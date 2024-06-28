using CommandLine;
using System.Xml;

namespace pace_converter
{
    internal class Program
    {
        enum DistanceUnit
        {
            miles, kilometers, yards
        }

        static Dictionary<string, double> DistanceToMeters()
        {
            Dictionary<string, double> dict = new();
            dict.Add("miles", 1609.344d);
            dict.Add("kilometers", 1000d);
            dict.Add("yards", 0.9144d);

            return dict;
        }

        class Options
        {
            [Option("input", 
                Required = true, 
                HelpText = "The input distance unit, default: miles. <miles | kilometers | yards>",
                Default = DistanceUnit.miles)]
            public DistanceUnit DistanceUnitIn { get; set; }

            [Option("output", 
                Required = true, 
                HelpText = "The output distance unit, default: miles. <miles | kilometers | yards>", 
                Default = DistanceUnit.miles)]
            public DistanceUnit DistanceUnitOut { get; set; }

            [Option("invert", 
                Required = false,
                HelpText = "Outputs to distance/time rather than time/distance, ex: minutes per mile to miles per minute",
                Default = false)]
            public bool Invert { get; set; }

            [Option("time", Required = true, HelpText = "Input time formatted: 'HH:mm:ss")]
            public TimeOnly Time { get; set; }

            [Option("distance", Required = true, HelpText = "Input distance")]
            public double Distance { get; set; }
        }

        static void Main(string[] args)
        {
            var toMeter = DistanceToMeters();

            Options options = new();
            Parser parser = new Parser();
            var result = parser.ParseArguments<Options>(args);
            options = result.Value!;

            var seconds = options.Time.ToTimeSpan().TotalSeconds;
            var distanceInMeters = 0d;
            switch (options.DistanceUnitIn)
            {
                case DistanceUnit.miles:
                    distanceInMeters = options.Distance * toMeter["miles"];
                    break;
                case DistanceUnit.kilometers:
                    distanceInMeters = options.Distance * toMeter["kilometers"];
                    break;
                case DistanceUnit.yards:
                    distanceInMeters = options.Distance * toMeter["yards"];
                    break;
            }

            var metersPerSecond = distanceInMeters / seconds;
            var finalMeasurement = 0d;
            switch (options.DistanceUnitOut)
            {
                case DistanceUnit.miles:
                    finalMeasurement = metersPerSecond * (1 / toMeter["miles"]);
                    break;
                case DistanceUnit.kilometers:
                    finalMeasurement = metersPerSecond * (1 / toMeter["kilometers"]);
                    break;
                case DistanceUnit.yards:
                    finalMeasurement = metersPerSecond * (1 / toMeter["yards"]);
                    break;
            }

            if (options.Invert) { finalMeasurement = 1 / finalMeasurement; };

            string unit = "";
            if (!options.Invert)
                unit = options.DistanceUnitOut.ToString() + "/second"; // calebx - need to add a TimeUnitOut option here.
            else
                unit = "seconds/" + options.DistanceUnitOut;

            string output = options.Invert ? (int)finalMeasurement + " " + unit : finalMeasurement.ToString("0.00000") + " " + unit;
            Console.WriteLine(output);
        }
    }
}
