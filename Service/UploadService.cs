using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnsekTechTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EnsekTechTest.Service
{
    public class UploadService
    {
        private readonly DatabaseContext _context;

        public UploadService(DatabaseContext context)
        {
            _context = context;
        }

        public async IAsyncEnumerable<LineUploadReport> ProcessFile(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            using (var stream = new StreamReader(memoryStream))
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var line = await stream.ReadLineAsync();
                FileStructure fileStructure = null;
                var fileLineNumber = 1;

                while (!string.IsNullOrEmpty(line))
                {
                    if (fileStructure == null)
                        fileStructure = BuildFileStructure(line);
                    else
                        yield return ProcessFileLine(fileStructure, line, fileLineNumber++);

                    line = await stream.ReadLineAsync();
                }
            }
        }

        private LineUploadReport ProcessFileLine(FileStructure fileStructure, string line, int fileLineNumber)
        {
            var lineSplit = line.Split(",");
            if (lineSplit.Length != 3)
                return new LineUploadReport
                {
                    LineIndex = fileLineNumber,
                    Status = UploadStatus.InvalidLine
                };

            var meterReading = new MeterReading();

            for (var idx = 0; idx < lineSplit.Length; idx++)
            {
                var dataField = lineSplit[idx];

                if (idx == fileStructure.AccountIdIndex)
                {
                    if (!int.TryParse(dataField, out var accountId))
                        return new LineUploadReport
                        {
                            LineIndex = fileLineNumber,
                            Status = UploadStatus.InvalidLine
                        };

                    meterReading.AccountId = accountId;
                }
                else if (idx == fileStructure.MeterReadValueIndex)
                {
                    if (!int.TryParse(dataField, out var meterReadingValue))
                        return new LineUploadReport
                        {
                            LineIndex = fileLineNumber,
                            Status = UploadStatus.InvalidLine
                        };

                    if (meterReadingValue < 0 || meterReadingValue > 99999)
                        return new LineUploadReport
                        {
                            LineIndex = fileLineNumber,
                            Status = UploadStatus.InvalidMeterReading
                        };

                    meterReading.MeterReadValue = meterReadingValue;
                }
                else if (idx == fileStructure.MeterReadingDateTimeIndex)
                {
                    if (!DateTime.TryParse(dataField, out var meterReadingDateTime))
                        return new LineUploadReport
                        {
                            LineIndex = fileLineNumber,
                            Status = UploadStatus.InvalidLine
                        };

                    meterReading.MeterReadingDateTime = meterReadingDateTime;
                }
            }

            var account = _context.Accounts.Include(a => a.MeterReadings)
                .SingleOrDefault(a => a.Id == meterReading.AccountId);

            if (account == null)
                return new LineUploadReport {LineIndex = fileLineNumber, Status = UploadStatus.UnknownAccount};

            if (account.MeterReadings.Any(mr =>
                    mr.MeterReadValue == meterReading.MeterReadValue &&
                    mr.MeterReadingDateTime == meterReading.MeterReadingDateTime))
                // would consider same reading on different days to be correct as account might have not used any within that timeframe.
                return new LineUploadReport {LineIndex = fileLineNumber, Status = UploadStatus.DuplicateMeterReading};


            _context.MeterReadings.Add(meterReading);

            _context.SaveChanges();

            return new LineUploadReport {LineIndex = fileLineNumber, Status = UploadStatus.Success};
        }

        private static FileStructure BuildFileStructure(string line)
        {
            var lineSplit = line.Split(",");
            var fileStructure = new FileStructure();

            for (var index = 0; index < lineSplit.Length; index++)
            {
                var s = lineSplit[index];
                switch (s)
                {
                    case nameof(MeterReading.AccountId):
                        fileStructure.AccountIdIndex = index;
                        break;
                    case nameof(MeterReading.MeterReadValue):
                        fileStructure.MeterReadValueIndex = index;
                        break;
                    case nameof(MeterReading.MeterReadingDateTime):
                        fileStructure.MeterReadingDateTimeIndex = index;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown field within file upload {s}");
                }
            }

            if (!fileStructure.IsValid()) throw new InvalidOperationException("Uploaded file structure not recognised");

            return fileStructure;
        }
    }
}