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

    /// <summary>
    /// Abstract class to convert a json data to the correct class.
    /// </summary>
    internal abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of the objectType.
        /// </summary>
        /// <param name="objectType">Type of object expected</param>
        /// <param name="jObject">Content of json data.</param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType) {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, 
                                        object existingValue, JsonSerializer serializer) {
            JObject jObject = JObject.Load(reader);
            T target = Create(objectType, jObject);
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value,
                                    JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// The job converter when reading json file.
    /// </summary>
    internal class JobConverter : JsonCreationConverter<Job> {

        /// <summary>
        /// Create the correct instance of Job.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <param name:"jObject">Content of json data.</param>
        /// <returns>The correct instance of Job.</returns>
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

        /// <summary>
        /// Returns the Type Name of the current data.
        /// </summary>
        private string GetFieldName(string fieldName, JObject jObject) {
            return jObject[fieldName].ToString();
        }
    }


    /// Class that keep in memory each jobs added.
    internal sealed class DataManager {

        /// <summary>
        /// List to save the job that the DataManager has to delete
        /// after testing every job.
        /// </summary>
        private LinkedList<Job> _jobListSave;

        /// <summary>
        /// List to save every job to execute if it's time.
        /// </summary>
        public LinkedList<Job> JobList {
            get;
            private set; 
        }

        /// <summary>
        /// Path to the json file, use to save and load every date for every
        /// jobs.
        /// </summary>
        public string DataPath {
            get;
            private set;
        }



        /// <summary>
        /// Construct the DataManager with the json path.
        /// </summary>
        /// <param name="DataPath">The json file path.</param>
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



        /// <summary>
        /// Add the job in the list.
        /// If the job already exists (ie. has the same Id than an another...),
        /// it throws an JobAlreadyExistsException.
        /// If the job is in the savelist (ie. loaded from the json file), then
        /// it will take the saved date; else it does nothing.
        /// Then it add the NewJob if the job list.
        /// </summary>
        /// <param name="NewJob">The new job to add.</param>
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


        /// <summary>
        /// Remove the job in the list.
        /// If the job doesn't exists then it throws an exception.
        /// It updates the json file.
        /// </summary>
        /// <param name ="JobToRemove">The job to remove.</param>
        public void RemoveJob(Job JobToRemove) {
            if (!Exists(JobToRemove))
                throw new ArgumentException(JobToRemove.Id + " doesn't exists.");

            JobList.Remove(JobToRemove);
            Update();
        }


        /// <summary>
        /// Update the json file.
        /// </summary>
        public void Update() {
            /* Save in a file */
            using(StreamWriter sw = File.CreateText(this.DataPath)) {
                JsonSerializer js = new JsonSerializer();  
                js.Serialize(sw, JobList);
            }
        
            /* Reload the file in the job list. */
            _jobListSave = JsonConvert.DeserializeObject<LinkedList<Job>>(
                    File.ReadAllText(this.DataPath), new JobConverter());
        }

    
        /// <summary>
        /// Function to know if the job exists in the job list.
        /// </summary>
        /// <param name="j">Job to find.</param>
        /// <returns>true if it exists</returns>
        public bool Exists(Job j) {
            foreach (Job job in JobList)  {
                if (job.Equals(j)) 
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Function to know if the job exists in the saved job list.
        /// </summary>
        /// <param name="j">Job to find.</param>
        /// <returns>The job if it exists</returns>
        private Job IsInSaveList(Job j) {
            foreach (Job job in _jobListSave) {
                if (job.Equals(j)) 
                    return job;
            }

            return null;
        }
    
    }




    /// Keep an abstraction to write logs (ie. size of log file, ...)
    internal sealed class LogWriter {

        private static uint LOGSIZE = 
            Convert.ToUInt32(Math.Pow(2.0, 16.0));

        /// <summary>
        /// Path to the log file.
        /// </summary>
        private string LogPath {
            get;
            set;
        }

        /// <summary>
        /// Construct the LogWriter with the given path.
        /// </summary>
        /// <param name="LogPath">Path to the logfile.</param>
        public LogWriter(string LogPath) {
            this.LogPath = LogPath;
        }

        /// <summary>
        /// Write into the logfile.
        /// Take a title and a body to make the log clearer.
        /// </summary>
        /// <param name="Title">Title of the message.</param>
        /// <param name="Body">Body of the message.</param>
        public void Write(string Title, string Body) {
            DateTime Now = DateTime.Now;

            string Message =
                Now + 
                Environment.NewLine +
                Title + 
                Environment.NewLine +
                Body + 
                Environment.NewLine +
                Environment.NewLine;

            string Text = "";

            if (File.Exists(LogPath)) {
                Text = File.ReadAllText(LogPath);
            }

            int size = Text.Length + Message.Length;
            if (size > LOGSIZE) {
                Text = Text.Substring(0, Text.Length - Message.Length);
            }

            File.WriteAllText(LogPath, Message + Text);
        }


    
    }





    /// The manager. It's here, with a timer, to execute jobs when it's needed.
    public sealed class Manager {

        internal DataManager _data;
        internal LogWriter   _log;
        internal Timer       _jobTimer;



        /// <summary>
        /// The data manager.
        /// It keeps the job list, and permitt to save and load from a json
        /// file.
        /// </summary>
        private DataManager Data {
            get { return _data;  } 
            set { _data = value; }
        }

        /// <summary>
        /// The log writer.
        /// </summary>
        private LogWriter Log {
            get { return _log;  } 
            set { _log = value; }
        }

        /// <summary>
        /// Timer to check if any job have to be executed.
        /// </summary>
        private Timer JobTimer {
            get { return _jobTimer;  } 
            set { _jobTimer = value; }
        }



        /// <summary>
        /// Construct the manager. Jobs can be added right after.
        /// <summary>
        /// <param name="JsonPathToFile">Path to the saved json file. It the
        /// file doesn't exist, it will be created.</param>
        /// <param name="LogPathToFile">Path to the log file. It the file
        /// does'nt exist, it will be created.</param>
        /// <param name="TimerInterval">The timer between two ticks of the
        /// clock, to check if some jobs have to be executed.</param>
        public Manager(string JsonPathToFile, string LogPathToFile, 
                int TimerInterval) {
            if (JsonPathToFile == null) 
                throw new ArgumentException("Json path can't be null.");

            Data = new DataManager(JsonPathToFile);

            Log = new LogWriter(LogPathToFile);

            JobTimer = new Timer(o => TimerTick(), new AutoResetEvent(true), 
                    10, TimerInterval);
        }


        /// <summary>
        /// Give the job to the manager.
        /// </summary>
        /// <param name="j">The job.</param>
        public void AddJob(Job j) {
            Data.AddJob(j);
        }

        /// <summary>
        /// Remove the job.
        /// </summary>
        /// <param name="j">The job to remove.</param>
        public void RemoveJob(Job j) {
            Data.RemoveJob(j); 
        }

        /// <summary>
        /// Update the data.
        /// </summary>
        public void Update() {
            Data.Update(); 
        }



        /// <summary>
        /// Function executed when the timer has reached its end.
        /// </summary>
        private void TimerTick() {
            DateTime Now = DateTime.Now;

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
                    }
                }
            }

            foreach (Job job in ToDeleteList) {
                Data.RemoveJob(job) ;
            }


        }
         
    }
    
}
