using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Timers;

namespace ValorantAnyaBot.Services
{
    public class AutomatedTaskService
    {
        private FileInfo _f;
        private Timer _timer;
        private TimeSpan sc;

        public AutomatedTaskSubscribedUser Users { get; set; }

        public AutomatedTaskService()
        {
            _f = new FileInfo("auto.json");
            Users = new AutomatedTaskSubscribedUser();
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += On_timerElapsed;
            sc = new TimeSpan(9, 0, 0);

            if (!_f.Exists)
            {
                File.Create(_f.FullName).Close();
                File.WriteAllText(
                    _f.FullName,
                    JsonSerializer.Serialize(
                        Users,
                        new JsonSerializerOptions()
                        {
                            IncludeFields = true
                        }));
            }

            Users = JsonSerializer.Deserialize<AutomatedTaskSubscribedUser>(
                _f.OpenText().BaseStream);
            _timer.Enabled = true;
            _timer.Start();
        }
        private async void On_timerElapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.TimeOfDay.Hours == sc.Hours &&
                DateTime.Now.TimeOfDay.Minutes == sc.Minutes &&
                DateTime.Now.TimeOfDay.Seconds == sc.Seconds)
            {
                foreach (ulong id in Users.users)
                {
                    await ValorantOfferService.ShowOfferEmbeds(id);
                }
            }
        }

        public bool Add(ulong id)
        {
            if (Users.users.Contains(id)) return false;
            Users.users.Add(id);
            Save();
            return true;
        }
        public bool Remove(ulong id)
        {
            Users.users.Remove(id);
            Save();
            return true;
        }

        public void Save()
        {
            Clear();
            File.WriteAllText(
                "auto.json",
                JsonSerializer.Serialize(
                    Users,
                    new JsonSerializerOptions()
                    {
                        IncludeFields = true
                    }));
        }

        private void Clear()
        {
            using (FileStream fs = new FileInfo("auto.json").Open(FileMode.Open))
            {
                fs.SetLength(0);
                fs.Flush();
            }
        }
    }
    public class AutomatedTaskSubscribedUser
    {
        public List<ulong> users { get; set; } = new List<ulong>();
    }
}
