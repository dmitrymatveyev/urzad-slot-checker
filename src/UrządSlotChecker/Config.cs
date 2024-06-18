namespace UrządSlotChecker
{
    public class Config
    {
        public string BrowserPath { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        private int _checkIntervalInSeconds;
        public int CheckIntervalInSeconds
        {
            get { return _checkIntervalInSeconds; }
            set
            {
                if (value < 1)
                {
                    _checkIntervalInSeconds = 1;
                }
                else
                {
                    _checkIntervalInSeconds = value;
                }
            }
        }

        public string URL { get; set; }

        public uint VisitTypeNumber { get; set; }

        public string LackOfSlotsMessage1 { get; set; }

        public string LackOfSlotsMessage2 { get; set; }

        public string ScreenshotsDir { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public bool SmtpEnableSsl { get; set; }

        public string FromEmail { get; set; }

        public string FromPassword { get; set; }

        public string ToEmail { get; set; }

        public string EmailSubject { get; set; }

        public string EmailContent { get; set; }
    }
}