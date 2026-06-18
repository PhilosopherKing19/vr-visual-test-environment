using System.Globalization;
using System.IO;
using UnityEngine;

// Shared CSV trial logger used by all task scripts (Visual Acuity, NASA-TLX,
// Matching, Comparative Search). Each task creates one TrialLogger instance,
// which owns a single timestamped CSV file for the session and writes the
// header on construction and one row per trial afterwards. Centralizing the
// logging here removes the duplicated csvPath / WriteAllText / AppendAllText
// code that previously lived in every task script, and guarantees that all
// values are written with InvariantCulture so floating-point numbers use a
// dot as the decimal separator regardless of the machine's system locale.
public class TrialLogger
{
    private readonly string csvPath;

    // Creates a new timestamped CSV file under Application.persistentDataPath,
    // named "<filePrefix>_<timestamp>.csv", and immediately writes the comma-
    // separated header row built from the provided column names.
    public TrialLogger(string filePrefix, string[] columns)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/" + filePrefix + "_" + timestamp + ".csv";
        File.WriteAllText(csvPath, string.Join(",", columns) + "\n");
    }

    // Appends one trial as a comma-separated line. Each value is converted with
    // InvariantCulture, so floats are formatted with a dot (e.g. "1.234"),
    // bools as "True"/"False", and enums as their name (e.g. "LandoltC"),
    // matching the output the task scripts produced before. The number and
    // order of values must match the columns passed to the constructor.
    public void WriteRow(params object[] values)
    {
        string[] cells = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
            cells[i] = System.Convert.ToString(values[i], CultureInfo.InvariantCulture);
        File.AppendAllText(csvPath, string.Join(",", cells) + "\n");
    }
}
