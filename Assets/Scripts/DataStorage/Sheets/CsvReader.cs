using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class CsvReader : MonoBehaviour {

    [SerializeField]
    public Table[] tables;

    private static string[] meta;

    public void ToJson() {
        foreach (var table in tables) {
            CsvToJson(table);
            CsvToJsonLine(table);
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    

   
    private void CsvToJson(Table table) {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(table.csvFile);
        string text = String.Empty;
        using (File.OpenRead(path)) {
            text = File.ReadAllText(path);
        }

        string[,] grid = SplitCsvGrid(text);
        string    json = Json(grid);

        string folder = Path.Combine(Application.streamingAssetsPath, table.folderPath.name);
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }

        var fullName = Path.Combine(folder, table.filename);

        if (!File.Exists(fullName)) {
            File.Create(fullName).Dispose();
        }

        File.WriteAllText(fullName, json);
#endif
    }
  

    private static string Json(string[,] grid) {
        string[] typeData = GetTypeData(grid);
        string   json     = "[";
        var      dimX     = grid.GetUpperBound(0);
        var      dimY     = grid.GetUpperBound(1);
        for (int y = 2; y < dimY; y++) {
            json += "{";
            for (int x = 0; x < dimX; x++) {
                if (typeData[x] == "IGNORE") {
                    continue;
                }

                if (String.IsNullOrEmpty(grid[x, y]) || grid[x, y] == "-") {
                    continue;
                }

                var line = WrapQuotes(grid[x, 0]);
                line += ": ";
                line += WrapType(grid[x, y], typeData[x]);
                json += line;
                if (x < dimX - 1) { json += ","; }
            }

            json =  RemoveLastExtraComma(json);
            json += "}";
            if (y < dimY - 1) { json += ","; }

            json += "\n";
        }

        json += "]";
        return json;
    }

    private void CsvToJsonLine(Table table) {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(table.csvFile);
        string text = String.Empty;
        using (File.OpenRead(path)) {
            text = File.ReadAllText(path);
        }

        string[,] grid = SplitCsvGrid(text);
        string[]  json = JsonLine(grid);

        string folder = Path.Combine(Application.streamingAssetsPath, table.folderPath.name);
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }

        var fullName = Path.Combine(folder, table.filename);

        for (var i = 0; i < json.Length; i++) {
            if (!File.Exists(fullName)) {
                File.Create(fullName).Dispose();
            }

            File.WriteAllText(fullName + meta[i], json[i]);
        }
#endif
    }

    private static string[] JsonLine(string[,] grid) {
        string[] typeData = GetTypeData(grid);
        var      dimX     = grid.GetUpperBound(0);
        var      dimY     = grid.GetUpperBound(1);
        string[] lines    = new string[dimY - 2];
        meta = new string[dimY - 2];
        for (int y = 2; y < dimY; y++) {
            string json = "{";
            for (int x = 0; x < dimX; x++) {
                if (typeData[x] == "IGNORE") {
                    meta[y - 2] = grid[x, y];
                    continue;
                }

                if (String.IsNullOrEmpty(grid[x, y]) || grid[x, y] == "-") {
                    continue;
                }

                var line = WrapQuotes(grid[x, 0]);
                line += ": ";
                line += WrapType(grid[x, y], typeData[x]);
                json += line;
                if (x < dimX - 1) { json += ","; }
            }
            json            =  RemoveLastExtraComma(json);
            json            += "}";
            lines[y - 2] =  json;
        }

        return lines;
    }

    private static string RemoveLastExtraComma(string s) {
        var n = s.Length - 1;
        return s[n] == ',' ? s.Remove(n) : s;
    }

    private static string WrapType(string s, string type) {
        switch (type) {
            case "":       return s;
            case "\"\"":   return WrapQuotes(s);
            case "[]":     return WrapBracers(s);
            case "IGNORE": return s;
            default:       return s;
        }
    }

    private static string WrapBracers(string s) { return "[" + s + "]"; }

    private static string[] GetTypeData(string[,] grid) {
        var      size  = grid.GetUpperBound(0);
        string[] types = new string[size];
        for (int x = 0; x < size; x++) {
            types[x] = grid[x, 1];
        }

        return types;
    }

    private static string WrapQuotes(string s) { return "\"" + s + "\""; }

    // splits a CSV file into a 2D string array
    private static string[,] SplitCsvGrid(string csvText) {
        string[] lines = csvText.Split("\n"[0]);

        // finds the max width of row
        int width = 0;
        for (int i = 0; i < lines.Length; i++) {
            string[] row = SplitCsvLine(lines[i]);
            width = Mathf.Max(width, row.Length);
        }

        // creates new 2D string grid to output to
        string[,] outputGrid = new string[width + 1, lines.Length + 1];
        for (int y = 0; y < lines.Length; y++) {
            string[] row = SplitCsvLine(lines[y]);
            for (int x = 0; x < row.Length; x++) {
                outputGrid[x, y] = row[x];

                // This line was to replace "" with " in my output. 
                // Include or edit it as you wish.
                outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputGrid;
    }

    // splits a CSV row 
    private static string[] SplitCsvLine(string line) {
        return (from Match m in Regex.Matches(line, @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }

    public static void DebugOutputGrid(string[,] grid) {
        string textOutput = "";
        for (int y = 0; y < grid.GetUpperBound(1); y++) {
            for (int x = 0; x < grid.GetUpperBound(0); x++) {
                textOutput += grid[x, y];
                textOutput += "|";
            }

            textOutput += "\n";
        }

        Debug.Log(textOutput);
    }
}