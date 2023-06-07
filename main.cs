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
    string left = String.Format("{0}\n\n{1}", FormatSpeed(), FormatStoppingDistance());
    string right = String.Format("{0}\n\n{1}", FormatThrusterEfficacy(), FormatCarryCapacity());

    PrintCockpitLCD(0, left);
    PrintCockpitLCD(2, right);
}

string FormatSpeed(){
    double mps = GetForwardMetersPerSecond();
    double kmph = ConvertMetersPerSecondToKilometersPerHour(mps);

    return String.Format("Forward Speed\n {0} m/s\n {1} km/h", RoundTwoDecimals(mps), RoundTwoDecimals(kmph));
}

string FormatRange(){
    // GetRuntimeRemainingSeconds();
    return "";
}

string FormatThrusterEfficacy(){
    List<IMyThrust> thrusters = GetThrusters();
    float ratioSum = 0;

    for (int i = 0; i < thrusters.Count; i++){
        float max = thrusters[i].MaxThrust;
        float eff = thrusters[i].MaxEffectiveThrust;

        float ratio = eff / max;
        ratioSum += ratio;
    }

    float avgRatio = ratioSum / thrusters.Count;

    return String.Format("Thruster Efficacy\n {0}%", RoundTwoDecimals(avgRatio * 100));;
}

string FormatCarryCapacity(){
    double force = GetMaxThrustInDirection(Vector3I.Down);
    double accel = GetGravity();

    double maxMass = force / accel;
    double curMass = GetTotalShipMass();

    if (maxMass >= 1000){
        return String.Format("Carry Capacity\n {0}t ({1}%)", RoundTwoDecimals(maxMass / 1000), RoundTwoDecimals(curMass/maxMass * 100));
    }
    else{
        return String.Format("Carry Capacity\n {0}kg ({1}%)", RoundTwoDecimals(maxMass), RoundTwoDecimals(curMass/maxMass * 100));
    }
}

string FormatStoppingDistance(){
    double force = GetMaxThrustInDirection(Vector3I.Forward);
    double mass = GetTotalShipMass();

    double velocity = GetForwardMetersPerSecond();
    double accel = force / mass;

    double distance = -(velocity * velocity) / (2 * -accel);

    return String.Format("Stopping Distance\n {0}m", RoundTwoDecimals(distance));
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

double GetGravity(){
    return shipController.GetTotalGravity().Length();
}

double GetMaxThrustInDirection(Vector3I direction){
    List<IMyThrust> thrusters = GetThrustersInDirection(direction);

    float sum = 0;
    for (int i = 0; i < thrusters.Count; i++){
        sum += thrusters[i].MaxEffectiveThrust;
    }

    return (double) sum;
}

List<IMyThrust> GetThrustersInDirection(Vector3I direction){
    List<IMyThrust> allThrusters = GetThrusters();
    List<IMyThrust> directionThrusters = new List<IMyThrust>();

    for (int i = 0; i < allThrusters.Count; i++){
        IMyThrust thruster = allThrusters[i];

        if (thruster.IsWorking && thruster.GridThrustDirection == direction){
            directionThrusters.Add(thruster);
        }
    }

    return directionThrusters;
}

List<IMyThrust> GetThrusters(){
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType(thrusters);
    return thrusters;
}

double GetForwardMetersPerSecond(){
    return GetMetersPerSecondInDirection(Base6Directions.Direction.Forward);
}

Vector3I DirectionToVector3I(Base6Directions.Direction direction){
    return shipController.Position + Base6Directions.GetIntVector(shipController.Orientation.TransformDirection(direction));
}

double GetMetersPerSecondInDirection(Base6Directions.Direction direction){
    Vector3I vector3I = DirectionToVector3I(direction);
    Vector3D vector = Vector3D.Normalize(Vector3D.Subtract(shipController.CubeGrid.GridIntegerToWorld(vector3I), shipController.GetPosition()));

	return Vector3D.Dot(shipController.GetShipVelocities().LinearVelocity, vector);
}

double GetTotalShipMass(){
    MyShipMass shipMass = shipController.CalculateShipMass();
    return (double) shipMass.TotalMass;
}

void PrintCockpitLCD(int index, string data){
    (shipController as IMyCockpit).GetSurface(index).WriteText(data);
}

void ClearDebug(){
    shipController.CustomData = "";
}

void PrintDebug(string data){
    shipController.CustomData = shipController.CustomData + data + "\n";
}