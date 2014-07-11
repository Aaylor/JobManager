using JobManager.Exceptions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace JobManager {

    internal abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType) {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, 
                                        object existingValue, JsonSerializer serializer) {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value,
                                    JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

    internal class JobConverter : JsonCreationConverter<Job> {

        protected override Job Create(Type objectType, JObject jObject) {
            string TypeName = GetFieldName("Name", jObject);

            switch (TypeName) {
                case "RepeatableJob":
                    return new RepeatableJob();
                case "UniqueJob":
                    return new UniqueJob();
                default:
                    throw new ArgumentException("Invalid TypeName (" 
                            + TypeName + ") found in Json");
            }

        }

        private string GetFieldName(string fieldName, JObject jObject) {
            return jObject[fieldName].ToString();
        }
    }


    internal class DataManager {

        private LinkedList<Job> _jobListSave;

        public LinkedList<Job> JobList {
            get;
            private set; 
        }

        public string DataPath {
            get;
            private set;
        }



        internal DataManager(string DataPath) {
            this.DataPath = DataPath; 

            if (File.Exists(DataPath)) {
                /* Nothing to do here. */
            } else {
                File.Create(DataPath);
            }


            _jobListSave = new LinkedList<Job>();
            JobList = new LinkedList<Job>();
        }



        public void AddJob(Job NewJob) {
            if (Exists(NewJob)) 
                throw new JobAlreadyExistsException(NewJob.Id + " already exists.");

            Job SavedJob = IsInSaveList(NewJob);
            if (SavedJob != null) {
                NewJob.ExecutionDate = SavedJob.ExecutionDate;
                NewJob.ExecutionInterval = SavedJob.ExecutionInterval;
            }

            JobList.AddLast(NewJob);
            Update();
        }

        public void RemoveJob(Job NewJob) {
            if (!Exists(NewJob))
                throw new ArgumentException(NewJob.Id + " doesn't exists.");

            JobList.Remove(NewJob);
            Update();
        }

        public void Update() {
            /* Save in a file */
            using(StreamWriter sw = File.CreateText(this.DataPath)) {
                JsonSerializer js = new JsonSerializer();  
                js.Serialize(sw, JobList);
            }
        
            /* Load file */
            _jobListSave = JsonConvert.DeserializeObject<LinkedList<Job>>(
                    File.ReadAllText(this.DataPath), new JobConverter());
        }

    
        public bool Exists(Job j) {
            foreach (Job job in JobList)  {
                if (job.Equals(j)) 
                    return true;
            }

            return false;
        }


        private Job IsInSaveList(Job j) {
            foreach (Job job in _jobListSave) {
                if (job.Equals(j)) 
                    return job;
            }

            return null;
        }
    
    }




   
    internal class LogWriter {

        private string LogPath {
            get;
            set;
        }

        private StreamWriter LogSw {
            get;
            set;
        }

        private FileStream fs = null;


        public LogWriter(string LogPath) {
            this.LogPath = LogPath;

            fs = File.Open(LogPath, FileMode.Append, FileAccess.Write);
            LogSw = new StreamWriter(fs);
        }

        ~LogWriter() {
            fs.Close();
            LogSw.Close();
        }

        public void Write(string Title, string Body) {
            DateTime Now = DateTime.Now;

            LogSw.Write(
                Now + "\n" +
                Title + "\n" +
                Body + "\n\n"
            );
        }


    
    }






    public sealed class Manager {

        internal DataManager _data;
        internal LogWriter   _log;
        internal Timer       _jobTimer;



        private DataManager Data {
            get { return _data;  } 
            set { _data = value; }
        }

        private LogWriter Log {
            get { return _log;  } 
            set { _log = value; }
        }

        private Timer JobTimer {
            get { return _jobTimer;  } 
            set { _jobTimer = value; }
        }


        public Manager(string JsonPathToFile, string LogPathToFile, 
                int TimerInterval) {
            if (JsonPathToFile == null) 
                throw new ArgumentException("Json path can't be null.");

            Data = new DataManager(JsonPathToFile);

            Log = new LogWriter(LogPathToFile);

            AutoResetEvent are = new AutoResetEvent(true);
            TimerCallback tcb  = TimerTick;
            JobTimer = new Timer(tcb, are, 0, TimerInterval);
        }


        public void AddJob(Job j) {
            Data.AddJob(j);
        }

        public void RemoveJob(Job j) {
            Data.RemoveJob(j); 
        }

        public void Update() {
            Data.Update(); 
        }




        private void TimerTick(object sender) {
            DateTime Now = DateTime.Now;

            // TODO: Find an another way to resolve
            // InvalidOperation...Exeception
            var ToDeleteList = new LinkedList<Job>();

            Log.Write("TimerTick function.", "Ticking. Time to check jobs.");

            foreach (Job job in Data.JobList) {
                if (Now > job.ExecutionDate) {

                    Log.Write("TimerTick function.", string.Format("{0}: execution.", job.Id));
                    job.Execute();

                    if (job.IsRepeatable()) {
                        Log.Write("TimerTick function.", string.Format("{0}: repetition.", job.Id));
                        job.ChangeToNextDate(); 
                    } else {
                        Log.Write("TimerTick function.", string.Format("{0}: removing job.", job.Id));
                        ToDeleteList.AddLast(job);
                        //Data.RemoveJob(job);
                    }
                }
            }

            foreach (Job job in ToDeleteList) {
                Data.RemoveJob(job) ;
            }


        }
         
    }
    
}
