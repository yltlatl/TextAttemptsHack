using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TextAttemptsHack
{
    class Program
    {
        static void Main(string[] args)
        {
            var xrefPath = args[0];
            var xrefFileName = xrefPath.Split(new[] { '\\' }).Last();
            var xrefDf = new DelimitedFile(xrefPath, "us-ascii", '\n', ',', ';', '"');
            xrefDf.GetNextRecord();
            var xrefDict = new Dictionary<string, string>();
            while (!xrefDf.EndOfFile)
            {
                var accountNumber = xrefDf.GetFieldByName("ACCOUNT_NBR");
                var accountId = xrefDf.GetFieldByName("ACCOUNT_ID");
                xrefDict.Add(accountNumber, accountId);
                xrefDf.GetNextRecord();
                if (xrefDf.CurrentLineNumber % 1000 == 0)
                {
                    Console.WriteLine("Processing {0}, line number {1}.", xrefFileName, xrefDf.CurrentLineNumber);
                }
            }

            var textAttemptsPath = args[1];
            var textAttemptsFileName = textAttemptsPath.Split(new[] { '\\' }).Last();
            var taDf = new DelimitedFile(textAttemptsPath, "us-ascii", '\n', ',', ';', '"');
            taDf.GetNextRecord();
            var foundAcctIds = new Dictionary<string, List<string>>();
            var missingAcctIds = new List<string>();
            while (!taDf.EndOfFile)
            {
                var attemptAcctNbr = taDf.GetFieldByName("ACCOUNT_NBR");
                if (xrefDict.ContainsKey(attemptAcctNbr))
                {
                    if (foundAcctIds.ContainsKey(attemptAcctNbr))
                    {
                        foundAcctIds[attemptAcctNbr].Add(xrefDict[attemptAcctNbr]);
                    }
                    else
                    {
                        foundAcctIds.Add(attemptAcctNbr, new List<string>() { xrefDict[attemptAcctNbr] });
                    }
                }
                else
                {
                    missingAcctIds.Add(attemptAcctNbr);
                }
                taDf.GetNextRecord();
                if (taDf.CurrentLineNumber % 1000 == 0)
                {
                    Console.WriteLine("Processing {0}, line number {1}.", textAttemptsFileName, taDf.CurrentLineNumber);
                }
            }
            var outputPath = args[2];
            var foundAccountFilePath = outputPath + "found_account_IDs.txt";
            using (var _str = new StreamWriter(foundAccountFilePath))
            {
                _str.AutoFlush = true;
                foreach (var kvp in foundAcctIds)
                {
                    var idList = string.Join("; ", kvp.Value);
                    _str.WriteLine("{0},{1}", kvp.Key, idList);
                }
            }
            var missingAccountFilePath = outputPath + @"\missing_account_IDs.txt";
            using (var _str = new StreamWriter(missingAccountFilePath))
            {
                _str.AutoFlush = true;
                foreach (var accountNumber in missingAcctIds)
                {
                    _str.WriteLine(accountNumber);
                }
            }
            Console.WriteLine("Completed processing {0} account numbers with {1} account ids found and {2} account ids not found.", taDf.CurrentLineNumber, foundAcctIds.Count, missingAcctIds.Count);
        }
    }
}
