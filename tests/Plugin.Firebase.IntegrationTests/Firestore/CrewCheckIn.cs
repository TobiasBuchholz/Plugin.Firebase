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
        public IList<CrewCheckInEmployee> Employees { get; set; } = null!;

        [FirestoreProperty("yardAssets")]
        public IList<CrewCheckInAsset> YardAssets { get; set; } = null!;

        [FirestoreProperty("clockInTime")]
        public string ClockInTime { get; set; } = null!;

        [FirestoreProperty("yardLocation")]
        public string YardLocation { get; set; } = null!;

        [FirestoreProperty("emergencyCheckIn")]
        public bool EmergencyCheckIn { get; set; }

        [FirestoreProperty("removedAssets")]
        public IList<CrewCheckInRemovedAsset> RemovedAssets { get; set; } = null!;

        [FirestoreProperty("logs")]
        public IList<CrewCheckInLog> LogEntries { get; set; } = null!;

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
        public string Name { get; set; } = null!;

        [FirestoreProperty("class")]
        public string Clazz { get; set; } = null!;

        [FirestoreProperty("crew")]
        public int Crew { get; set; }

        [FirestoreProperty("languages")]
        public IList<string> Languages { get; set; } = null!;

        [FirestoreProperty("assignedEquipment")]
        public IList<CrewCheckInAsset> AssignedEquipment { get; set; } = null!;

        [FirestoreProperty("assignedVehicles")]
        public IList<CrewCheckInAsset> AssignedVehicles { get; set; } = null!;

        [FirestoreProperty("clockInTime")]
        public string ClockInTime { get; set; } = null!;

        [FirestoreProperty("status")]
        public string Status { get; set; } = null!;

        [FirestoreProperty("workType")]
        public string WorkType { get; set; } = null!;

        [FirestoreProperty("jobNumbers")]
        public IList<string> JobNumbers { get; set; } = null!;

        [FirestoreProperty("notes")]
        public string Notes { get; set; } = null!;
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
        public string AssetName { get; private set; } = null!;

        [FirestoreProperty("assetDescription")]
        public string AssetDescription { get; private set; } = null!;

        [FirestoreProperty("reason")]
        public string Reason { get; private set; } = null!;
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
        public string Action { get; set; } = null!;

        [FirestoreProperty("message")]
        public string Message { get; set; } = null!;
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
        public string Description { get; set; } = null!;

        [FirestoreProperty("name")]
        public string Name { get; set; } = null!;

        [FirestoreProperty("operator")]
        public string Operator { get; set; } = null!;

        [FirestoreProperty("type")]
        public string Type { get; set; } = null!;
    }
}