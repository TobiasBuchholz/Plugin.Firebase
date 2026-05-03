using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public class CrewCheckIn : IFirestoreObject
    {
        [Preserve]
        public CrewCheckIn()
        {
        }

        public CrewCheckIn(List<CrewCheckInEmployee> employees, List<CrewCheckInAsset> yardAssets, string clockInTime, string yardLocation, bool emergencyCheckIn, List<CrewCheckInRemovedAsset> removedAssets, List<CrewCheckInLog> logEntries, DateTime timestamp)
        {
            Employees = employees;
            YardAssets = yardAssets;
            ClockInTime = clockInTime;
            YardLocation = yardLocation;
            EmergencyCheckIn = emergencyCheckIn;
            RemovedAssets = removedAssets;
            LogEntries = logEntries;
            Timestamp = timestamp;
        }

        [FirestoreProperty("employees")]
        public IList<CrewCheckInEmployee> Employees { get; set; }

        [FirestoreProperty("yardAssets")]
        public IList<CrewCheckInAsset> YardAssets { get; set; }

        [FirestoreProperty("clockInTime")]
        public string ClockInTime { get; set; }

        [FirestoreProperty("yardLocation")]
        public string YardLocation { get; set; }

        [FirestoreProperty("emergencyCheckIn")]
        public bool EmergencyCheckIn { get; set; }

        [FirestoreProperty("removedAssets")]
        public IList<CrewCheckInRemovedAsset> RemovedAssets { get; set; }

        [FirestoreProperty("logs")]
        public IList<CrewCheckInLog> LogEntries { get; set; }

        [FirestoreProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public class CrewCheckInEmployee : IFirestoreObject
    {
        [Preserve]
        public CrewCheckInEmployee()
        {
        }

        public CrewCheckInEmployee(string name, string clazz, int crew, List<string> languages, List<CrewCheckInAsset> assignedEquipment, List<CrewCheckInAsset> assignedVehicles, string clockInTime, string status, string workType, List<string> jobNumbers, string notes)
        {
            Name = name;
            Clazz = clazz;
            Crew = crew;
            Languages = languages;
            AssignedEquipment = assignedEquipment;
            AssignedVehicles = assignedVehicles;
            ClockInTime = clockInTime;
            Status = status;
            WorkType = workType;
            JobNumbers = jobNumbers;
            Notes = notes;
        }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("class")]
        public string Clazz { get; set; }

        [FirestoreProperty("crew")]
        public int Crew { get; set; }

        [FirestoreProperty("languages")]
        public IList<string> Languages { get; set; }

        [FirestoreProperty("assignedEquipment")]
        public IList<CrewCheckInAsset> AssignedEquipment { get; set; }

        [FirestoreProperty("assignedVehicles")]
        public IList<CrewCheckInAsset> AssignedVehicles { get; set; }

        [FirestoreProperty("clockInTime")]
        public string ClockInTime { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; }

        [FirestoreProperty("workType")]
        public string WorkType { get; set; }

        [FirestoreProperty("jobNumbers")]
        public IList<string> JobNumbers { get; set; }

        [FirestoreProperty("notes")]
        public string Notes { get; set; }
    }

    public class CrewCheckInRemovedAsset : IFirestoreObject
    {
        [Preserve]
        public CrewCheckInRemovedAsset()
        {
        }

        public CrewCheckInRemovedAsset(string assetName, string assetDescription, string reason)
        {
            AssetName = assetName;
            AssetDescription = assetDescription;
            Reason = reason;
        }

        [FirestoreProperty("assetName")]
        public string AssetName { get; private set; }

        [FirestoreProperty("assetDescription")]
        public string AssetDescription { get; private set; }

        [FirestoreProperty("reason")]
        public string Reason { get; private set; }
    }

    public class CrewCheckInLog : IFirestoreObject
    {
        [Preserve]
        public CrewCheckInLog()
        {
        }

        public CrewCheckInLog(DateTime timestamp, string action, string message)
        {
            Timestamp = timestamp;
            Action = action;
            Message = message;
        }

        [FirestoreProperty("timestamp")]
        public DateTime Timestamp { get; set; } // Plugin.Firebase maps to Firestore timestamp

        [FirestoreProperty("action")]
        public string Action { get; set; }

        [FirestoreProperty("message")]
        public string Message { get; set; }
    }

    public class CrewCheckInAsset : IFirestoreObject
    {
        [Preserve]
        public CrewCheckInAsset()
        {
        }

        public CrewCheckInAsset(string description, string name, string @operator, string type)
        {
            Description = description;
            Name = name;
            Operator = @operator;
            Type = type;
        }

        [FirestoreProperty("description")]
        public string Description { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("operator")]
        public string Operator { get; set; }

        [FirestoreProperty("type")]
        public string Type { get; set; }
    }
}
