const UpdateFrequency DEFAULT_UPDATE_FREQUENCY = UpdateFrequency.Update10;
const int MAX_DEBUG_LINES = 100;
const string COCKPIT_NAME = "CS - Industrial Cockpit";

IMyShipController shipController;

public Program(){
    Runtime.UpdateFrequency = DEFAULT_UPDATE_FREQUENCY;
    shipController = GridTerminalSystem.GetBlockWithName(COCKPIT_NAME) as IMyShipController;

    ClearDebug();
}

public void Main(string argument, UpdateType updateSource){
    ClearDebug();

    string output = 
        FormatSpeed() + "\n" +
        FormatRange() + "\n" +
        FormatThrusterEfficacy() + "\n" +
        FormatCarryCapacity() + "\n" +
        FormatStoppingDistance();

    PrintDebug(output);
}

string FormatSpeed(){
    double mps = GetMetersPerSecond();
    double kmph = ConvertMetersPerSecondToKilometersPerHour(mps);

    return String.Format("Speed\n-{0} m/s\n-{1} km/h", RoundTwoDecimals(mps), RoundTwoDecimals(kmph));
}

string FormatRange(){
    GetRuntimeRemainingSeconds();
    return "";
}

string FormatThrusterEfficacy(){
    List<IMyThrust> thrusters = GetThrusters();

    return thrusters.Count.ToString();
}

string FormatCarryCapacity(){
    return "";
}

string FormatStoppingDistance(){
    return "";
}

double RoundTwoDecimals(double value){
    return Math.Round(value, 2);
}

string GetPercentString(double value){
    return RoundTwoDecimals(value * 100).ToString() + "%";
}

double ConvertMetersPerSecondToKilometersPerHour(double mps){
    return mps / 1000 * 3600;
}

List<IMyGasTank> GetUsefulGasolineTanks(){
    List<IMyGasTank> tanks = new List<IMyGasTank>();
    GridTerminalSystem.GetBlocksOfType(tanks);

    List<IMyGasTank> gasolineTanks = new List<IMyGasTank>();
    for (int i = 0; i < tanks.Count; i++){
        IMyGasTank tank = tanks[i];
        if (tank.DefinitionDisplayNameText.Contains("Gasoline")){
            if (tank.IsWorking && !tank.Stockpile){
                gasolineTanks.Add(tank);
            }
        }
    }

    return gasolineTanks;
}

List<IMyPowerProducer> GetUsefulGasolineEngines(){
    List<IMyPowerProducer> engines = new List<IMyPowerProducer>();
    GridTerminalSystem.GetBlocksOfType(engines);

    List<IMyPowerProducer> gasolineEngines = new List<IMyPowerProducer>();
    for (int i = 0; i < engines.Count; i++){
        IMyPowerProducer engine = engines[i];
        if (engine.DefinitionDisplayNameText.Contains("Gasoline")){
            if (engine.IsWorking){
                gasolineEngines.Add(engine);
            }
        }
    }

    return gasolineEngines;
}

double GetRuntimeRemainingSeconds(){
    List<IMyGasTank> tanks = GetUsefulGasolineTanks();
    List<IMyPowerProducer> engines = GetUsefulGasolineEngines();

    

    return 0;
}

List<IMyThrust> GetThrusters(){
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType(thrusters);
    return thrusters;
}

double GetMetersPerSecond(){
    return shipController.GetShipSpeed();
}

double GetTotalShipMass(){
    MyShipMass shipMass = shipController.CalculateShipMass();
    return (double) shipMass.TotalMass;
}

void ClearDebug(){
    shipController.CustomData = "";
}

void PrintDebug(string data){
    shipController.CustomData = shipController.CustomData + "\n" + data;
}