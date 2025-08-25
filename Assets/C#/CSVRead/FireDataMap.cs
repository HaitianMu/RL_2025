using CsvHelper.Configuration;
using System.Globalization;

// 这个类专门配置如何将CSV列映射到FireData属性
public sealed class FireDataMap : ClassMap<FireData>
{
    public FireDataMap()
    {
        Map(m => m.X).Index(0);
        Map(m => m.Y).Index(1);
        Map(m => m.Z).Index(2);
        Map(m => m.COConcentration).Index(3)
            .TypeConverterOption.NumberStyles(NumberStyles.Float)
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
        Map(m => m.Temperature).Index(4)
            .TypeConverterOption.NumberStyles(NumberStyles.Float)
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
        Map(m => m.Visibility).Index(5)
            .TypeConverterOption.NumberStyles(NumberStyles.Float)
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
    }
}