namespace SignalRChatApplication
{
    public static class Utility
    {
        public static void LogStuff(string stuff, string folderName)
        {
            var directory = $"C:/{folderName}/";
            var fileName = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.DayOfYear.ToString() + "_" + "Log.txt";
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.AppendAllText(directory + fileName, Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + stuff + Environment.NewLine + Environment.NewLine);
        }
    }
}
