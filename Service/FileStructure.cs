namespace EnsekTechTest.Service
{
    internal class FileStructure
    {
        public FileStructure()
        {
            AccountIdIndex = int.MinValue;
            MeterReadValueIndex = int.MinValue;
            MeterReadingDateTimeIndex = int.MinValue;
        }

        public int AccountIdIndex { get; set; }
        public int MeterReadValueIndex { get; set; }
        public int MeterReadingDateTimeIndex { get; set; }

        public bool IsValid()
        {
            return AccountIdIndex != int.MinValue
                   && MeterReadValueIndex != int.MinValue
                   && MeterReadingDateTimeIndex != int.MinValue
                   && AccountIdIndex != MeterReadValueIndex
                   && AccountIdIndex != MeterReadingDateTimeIndex
                   && MeterReadValueIndex != MeterReadingDateTimeIndex;
        }
    }
}