using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using System.IO;
using System.Windows.Forms;
using GTA.Math;
using NativeUI;
using System.Drawing;

namespace Street_Races
{
    public enum NitroUse
    {
        Safe,
        Normal,
        Aggresive,
    }

    public class Racer
    {
        public Vehicle car;
        public Ped driver;
        public Vector3 destination;

        //Waypoint & Position
        public int CurrentWaypoint = 0;
        public float DistanceToWaypoint = 0;
        public int PositionInRace = 1; //Handled by StreetRacing.HandlePositions();
        public int CurrentLap = 0;
        public int CurrentCheckpoint = 0;
        public string Name = "Racer";
        public int StuckTimer = 0;
        public float MaxSpeed = 120f;
        public bool finished = false;
        public bool being_careful = false;
        public bool Offroading = false;
        public bool Flat = false;
        public int TemporaryLocal = 0;
        //Personality
        NitroUse NitroUse = NitroUse.Normal;

        bool OutOfRace = false;

        //Nitro
        float NitroBar = 100;
        int NitroSafety = 0;
        int IntervalBetweenNitros = 4000;

        int NitroTimeLimit = Game.GameTime;


        //AI Safety
        int BrakeTime = 0;
       public int HoldBrakeTime = 0;
      public  float BrakeTargetspeed = 0f;
        public Racer(Vehicle model, Ped racer, string name)
        {
            if (Util.CanWeUse(racer))
            {
                Name = name;
                car = racer.CurrentVehicle;
                driver = racer;
                driver.SetIntoVehicle(car, VehicleSeat.Driver);

                if (!driver.IsPlayer)
                {

                    Function.Call(GTA.Native.Hash.SET_DRIVER_AGGRESSIVENESS, driver, 0);
                    Function.Call(GTA.Native.Hash.SET_DRIVER_ABILITY, driver, 10f);

                    driver.AlwaysKeepTask = true;
                    driver.BlockPermanentEvents = true;

                    Blip blip = car.AddBlip();
                    blip.Color = BlipColor.Blue;
                    blip.Name = Name;

                    Function.Call(GTA.Native.Hash._0x0DC7CABAB1E9B67E, driver, true); //Load Collision
                    Function.Call(GTA.Native.Hash._0x0DC7CABAB1E9B67E, car, true); //Load Collision

                    car.EngineRunning = true;
                }
                if (StreetRaces.AIGodmode.Checked)
                {
                    Function.Call(Hash.SET_VEHICLE_STRONG, car, true);

                    car.IsInvincible = true;
                    car.IsAxlesStrong = true;
                    car.IsCollisionProof = true;
                    car.IsBulletProof = true;
                    car.IsExplosionProof = true;
                    car.IsFireProof = true;
                    if (driver.IsPlayer)
                    {
                        driver.IsInvincible = true;
                        driver.IsCollisionProof = true;
                        driver.IsBulletProof = true;
                        driver.IsExplosionProof = true;
                        driver.IsFireProof = true;
                    }
                }
            }
        }
        public void Tick()
        {

            if (driver.IsPlayer) return;
            if (car.Speed>40 && Math.Abs(car.SteeringAngle) > 20)
            {
                UI.Notify(car.FriendlyName + " is freaking out.");
                Vector3 pos = car.Position;
                TaskSequence TempSequence = new TaskSequence();
                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 1, 2000);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, car, pos.X, pos.Y, pos.Z, 200f, 4194304, 250f);

                TempSequence.Close();
                driver.Task.PerformSequence(TempSequence);
                TempSequence.Dispose();
            }
            if (car.IsOnAllWheels && Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, car))
            {
                NitroSafety = 0;
            }

            if (LetOffTheGas && HoldBrakeTime > Game.GameTime && car.Speed>4f)
            {
                car.EngineTorqueMultiplier = 0f;
            }
            HandleNitro();
        }

        bool Nitroing = false;
        void HandleNitro()
        {
            if (NitroTimeLimit>Game.GameTime)
            {
                if (Function.Call<bool>((Hash)0x36D782F68B309BDA, car))
                {
                    if (!Function.Call<bool>((Hash)0x3D34E80EED4AE3BE, car))
                    {
                        Function.Call((Hash)0x81E1552E35DC3839, car, true);
                    }
                }
                else if(car.Acceleration>0)
                {
                    Util.ForceNitro(car);
                }
                else
                {
                    NitroSafety -= 4;
                }
            }
        }
        public void ProcessRace()
        {
            switch (StreetRaces.RaceStatus)
            {
                case RacePhase.NotRacing:
                    {

                        break;
                    }
                case RacePhase.RaceSetup:
                    {

                        if (!driver.IsPlayer)
                        {
                            if (driver.TaskSequenceProgress == -1)
                            {
                                Vector3 pos = car.Position;
                                //int d = Util.GetRandomInt(2, 5) * 1000;
                                TaskSequence TempSequence = new TaskSequence();
                                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 30, 4000);
                                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, car, pos.X, pos.Y, pos.Z, 200f, 4194304, 250f);
                                //                                Function.Call(Hash.TASK_PAUSE, 0, 1000);

                                TempSequence.Close();
                                driver.Task.PerformSequence(TempSequence);
                                TempSequence.Dispose();
                                UI.Notify(car.FriendlyName + " burnouts");
                            }
                        }
                            break;
                    }
                case RacePhase.RaceCountDown:
                    {
                        if (!driver.IsPlayer)
                        {
                            if (driver.TaskSequenceProgress == -1)
                            {
                                Vector3 pos = car.Position;

                              //  int d = Util.GetRandomInt(2, 5)*1000;
                                TaskSequence TempSequence = new TaskSequence();
                                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 30, 4000);
                                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, car, pos.X, pos.Y, pos.Z, 200f, 4194304, 250f);
//                                Function.Call(Hash.TASK_PAUSE, 0, 1000);

                                TempSequence.Close();
                                driver.Task.PerformSequence(TempSequence);
                                TempSequence.Dispose();
                                UI.Notify(car.FriendlyName + " burnouts");
                            }
                            if (car.IsConvertible)
                            {
                                if (Util.IsRaining() && car.RoofState == VehicleRoofState.Opened) car.RoofState = VehicleRoofState.Closing;
                                else if (car.RoofState == VehicleRoofState.Closed) car.RoofState = VehicleRoofState.Opening;
                            }
                        }
                        

                        break;
                    }
                case RacePhase.RaceInProgress:
                    {
                        if (!finished)
                        {
                            if (!OutOfRace) HandleStuck();                        
                            HandleRacing();
                        }
                        break;
                    }

            }
        }
        public void Delete()
        {
            if (!driver.IsPlayer)
            {
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, driver, car, 15f, 2);
                if (car.CurrentBlip != null) car.CurrentBlip.Remove();
                driver.MarkAsNoLongerNeeded();
                car.MarkAsNoLongerNeeded();
            }
        }
        public void HandleStuck()
        {
            if (car.Speed < 3f && car.Acceleration != 0)
            {
                if (StuckTimer < 500) StuckTimer++; else
                {
                    if (!car.IsOnScreen && car.Position.DistanceTo(Game.Player.Character.Position) > 50)
                    {
                        if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(Name + "is stuck, teleporting");

                        car.Position = destination.Around(15);
                        
                        car.PlaceOnGround();
                    }
                    else
                    {
                        //UI.Notify(Name +"is stuck, driving back");
                        //Vector3 pos = car.Position + (car.ForwardVector * -6);
                        //Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, driver, car, pos.X, pos.Y, pos.Z, MaxSpeed, 0, car.Model.Hash, 4+8+16+36+512, 2f, 1f);

                    }
                }
            }
            else
            {
                if (StuckTimer > 0) StuckTimer = 0;
            }

        }


        bool Handbrake = false;
        public void HandleRacing()
        {
            if (Offroading && !driver.IsPlayer)
            {
                if (TemporaryLocal == 0 && Util.CheckForObstaclesAhead(car))
                {
                    TemporaryLocal = Game.GameTime + 2000;
                    driver.DrivingStyle = (DrivingStyle) StreetRaces.BaseDrivingStyle + 4194304;
                    if (StreetRaces.Debug) UI.Notify(car.FriendlyName+ " set to temporary local");
                    NitroSafety = 0;
                }
                if (TemporaryLocal != 0 && TemporaryLocal < Game.GameTime && car.Speed>20f)
                {
                    if(Util.CheckForObstaclesAhead(car)) TemporaryLocal = Game.GameTime + 2000;
                    else
                    {
                       if(StreetRaces.Debug) UI.Notify(car.FriendlyName + " returned to fast");

                        TemporaryLocal = 0;
                        driver.DrivingStyle = (DrivingStyle)StreetRaces.BaseDrivingStyle + 16777216;
                    }
                }
            }

            //Debug notifications
            bool shouldnotify = false;
            if (StreetRaces.Debug && car.IsNearEntity(Game.Player.Character, new Vector3(30, 30, 30))) shouldnotify = true;

            string notification = "Lap: " + CurrentLap + "/" + StreetRaces.Laps + " | Waypoint: " + CurrentWaypoint;

            if (!driver.IsPlayer)
            {
                if (!car.Model.IsHelicopter && !car.Model.IsPlane)
                {
                  if(car.IsInRangeOf(Game.Player.Character.Position, 200))  HandleBrakes();

                    if (StreetRaces.AICatchup.Checked)
                    {
                        Vector3 playerpos = Game.Player.Character.Position;
                        if (!driver.IsPlayer && !car.IsOnScreen && Util.IsPlayerParticipating() && PositionInRace > 1 && Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, driver, playerpos.X, playerpos.Y, playerpos.Z).Y > 20f) // 
                        {
                            if (!car.IsInvincible) car.IsInvincible = true;
                            Util.HandleAICatchUp(car);
                        }
                        else if (car.IsInvincible) car.IsInvincible = false;
                    }


                    //Vehicle stuck handler
                    if (car.IsUpsideDown && car.IsStopped) car.PlaceOnGround();
                }

                if (StreetRaces.UseNitro.Checked)
                {
                    if (NitroTimeLimit > Game.GameTime)
                    {
                        //Calculate safety when nitroing
                        if (Util.SlidingTreshold(car, 3f) || Math.Abs(car.SteeringAngle) > 10)
                        {
                            if (NitroSafety > 40) NitroSafety = NitroSafety - 5;
                        }


                        //Nitro cancel conditions
                        if (NitroSafety < 50 || (NitroBar <= 0)) NitroTimeLimit = Game.GameTime - 1;



                        if (car.Acceleration > 0)
                        {
                            if (NitroBar >= 0 && NitroSafety>50)
                            {
                                //notification = notification + "~n~Nitroing";
                                NitroBar -= 2f;
                            }
                        } else if(NitroSafety > 0) NitroSafety -=1;

                    }
                    else
                    {
                        //Calculate safety when not nitroing
                        if (Util.SlidingTreshold(car, 20f))
                        {
                            if (NitroSafety > 40) NitroSafety = NitroSafety - 5;
                        }
                        else if (NitroSafety < 100 && car.Acceleration > 0)
                        {

                            if(Util.SlidingTreshold(car, 4f) || Math.Abs(car.SteeringAngle) > 5)
                            {
                              if(NitroSafety<50)  NitroSafety += 1;
                            }
                            else 
                            {
                                if (car.CurrentGear < 3) NitroSafety = NitroSafety + 5;
                                else NitroSafety += 2;
                            }

                        }

                        //Nitro recharge
                        if (NitroBar < 100) NitroBar = NitroBar + 1f;

                        //Generate next nitro session
                        if (NitroSafety > 60 && NitroTimeLimit + IntervalBetweenNitros < Game.GameTime && NitroBar>30)
                        {
                            int d = (Util.GetRandomInt(2, 5) * 1000);
                            NitroTimeLimit = Game.GameTime + d;
                       //     if (Game.Player.Character.IsSittingInVehicle(car)) UI.Notify("~g~Nitro time limit updated, +" + d / 1000 + "s");

                        }

                    }
                    if (NitroSafety < 50 && Function.Call<bool>((Hash)0x3D34E80EED4AE3BE, car)) Function.Call((Hash)0x81E1552E35DC3839, car, false);

                    //dev info
                    if (shouldnotify && World.RenderingCamera.IsActive)
                    {
                        notification = notification + "~n~Nitro bar: " + NitroBar + "%";
                        string safety = "~r~" + NitroSafety + "%";
                        if (NitroSafety > 10) safety = "~y~" + NitroSafety + "%";
                        if (NitroSafety > 50) safety = "~g~" + NitroSafety + "%";
                        notification = notification + "~n~Nitro Safety: " + safety + "~w~";
                        notification = notification + "~n~StrAng:" + Math.Round(car.SteeringAngle).ToString();
                    }




                    //Apply the nitro
                    if (NitroTimeLimit > Game.GameTime && car.Acceleration > 0)
                    {
                        if (Function.Call<bool>((Hash)0x36D782F68B309BDA, car))
                        {
                            if (Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y) > 10f)
                            {
                                //                        notification = notification + "~n~Nitroing (rocket)";
                                /*
                                if(!Function.Call<bool>((Hash)0x3D34E80EED4AE3BE, car))
                                {
                                    if (!Nitroing) Nitroing = true;
                                }
                                else
                                {
                                    if (Nitroing) Nitroing =false;
                                }*/
                                //Function.Call((Hash)0xFEB2DDED3509562E, car, 100f);
                            }
                        }
                        else
                        {



                        }
                    }
                    if (Game.Player.Character.IsSittingInVehicle(car) && StreetRaces.DebugMode.Checked) UI.ShowSubtitle("~g~b " + NitroBar + "- s " + NitroSafety);

                    if (NitroBar < 0) NitroBar = 0;

                }
                else shouldnotify = false;

            }


            //Waypoint following
            if (driver.IsAlive && car.EngineHealth>0)
            {
                if (StreetRaces.RaceWaypoints.Count > 0 && CurrentLap <= StreetRaces.Laps)
                {
                    if ((!driver.IsPlayer && !Util.IsDriving(driver)) || (driver.IsPlayer && driver.Position.DistanceTo(StreetRaces.RaceWaypoints[CurrentWaypoint]) < 20f))
                    {
                        being_careful = false; //Reset dangerous area braking
                        Offroading = false; //Reset offroading behavior
                        Flat = false;
                        int DrivingStyle = StreetRaces.BaseDrivingStyle;
                        //float speed = 120f;
                        if (driver.IsPlayer) StreetRaces.RaceWaypointBlips[CurrentCheckpoint].Scale = 0.7f;

                        CurrentCheckpoint++;
                        if (CurrentWaypoint == StreetRaces.RaceWaypoints.Count - 1)
                        {
                            CurrentWaypoint = 0;
                            if (!driver.IsPlayer)
                            {
                                Vector3 pos = StreetRaces.RaceWaypoints[CurrentWaypoint];
                                //driver.Task.DriveTo(car, pos, 15f, speed, DrivingStyle);
                                //Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, driver, car, pos.X, pos.Y, pos.Z, speed, DrivingStyle, 15f);
                                TaskSequence TempSequence = new TaskSequence();
                                if (car.IsStopped) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 1, Util.GetRandomInt(1, 8) * 100);
                                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, 0, car, pos.X, pos.Y, pos.Z, MaxSpeed, 0, car.Model.Hash, DrivingStyle, 14f, 13f);
                                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 1, 3000);
                                TempSequence.Close();
                                driver.Task.PerformSequence(TempSequence);
                                TempSequence.Dispose();
                            }
                            else
                            {
                                StreetRaces.RaceWaypointBlips[CurrentWaypoint].Scale = 1f;
                                StreetRaces.RaceWaypointBlips[StreetRaces.RaceWaypointBlips.Count - 1].Scale = 0.7f;
                            }
                        }
                        else
                        {
                            if ((driver.Position.DistanceTo(StreetRaces.RaceWaypoints[CurrentWaypoint]) < 20f || (CurrentLap == 0 && CurrentWaypoint == 0)))
                            {
                                //Add a lap
                                if (CurrentWaypoint == 0)
                                {
                                    CurrentCheckpoint = 0;
                                    CurrentLap++;
                                    if (CurrentLap > StreetRaces.Laps)
                                    {
                                        StreetRaces.RacersFinished++;
                                        finished = true;
                                        UI.Notify("~b~"+Name+"~w~ finished ~g~" + StreetRaces.RacersFinished+"º~w~.");
                                        if(StreetRaces.Winner == null) StreetRaces.Winner = this;
                                        //Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, driver, car, 0f, DrivingStyle);
                                        return;
                                    }
                                    else if (driver.IsPlayer && CurrentLap > 1) BigMessageThread.MessageInstance.ShowColoredShard(CurrentLap.ToString(), "", HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_MENU_YELLOW, 800);
                                }
                                CurrentWaypoint++;


                                //Check if its offroad and apply offroad driving style
                                if (CurrentWaypoint > 0 && StreetRaces.RaceOffroadWaypoints.Count>0)
                                {
                                    int Max = StreetRaces.RaceOffroadWaypoints.Count;
                                    int Current = CurrentWaypoint;

                                    if (Max>Current && StreetRaces.RaceOffroadWaypoints.Contains(StreetRaces.RaceWaypoints[CurrentWaypoint]))
                                    {
                                        DrivingStyle = StreetRaces.BaseDrivingStyle + 16777216;//4194304
                                        Offroading = true;
                                    }
                                }

                                //Check if car can see waypoint directly, if so make it drive straight to it
                                if (car.Position.DistanceTo(StreetRaces.RaceWaypoints[CurrentWaypoint]) < 50f || StreetRaces.RaceFlatWaypoints.Contains(StreetRaces.RaceWaypoints[CurrentWaypoint]))
                                {
                                    Flat = true;
                                    DrivingStyle = StreetRaces.BaseDrivingStyle + 16777216;
                                    Offroading = true;

                                }


                                //if (Util.ForwardSpeed(car) > 25f && StreetRaces.RaceDangerousWaypoints.Contains(StreetRaces.RaceWaypoints[CurrentWaypoint])) MaxSpeed = car.Speed / 1.5f; else MaxSpeed = 120f;// speed = car.Speed / 1.5f;

                                if (!driver.IsPlayer)
                                {
                                    Vector3 pos = StreetRaces.RaceWaypoints[CurrentWaypoint];
                                    //driver.Task.DriveTo(car, pos, 15f, speed, DrivingStyle);
                                    destination = pos;
                                    //if (CurrentLap == 1 && CurrentWaypoint < 3 && PositionInRace > 5) DrivingStyle += 1;
                                    TaskSequence TempSequence = new TaskSequence();
                                    //if (car.IsStopped) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 1, Util.GetRandomInt(1, 8) * 100);
                                    Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, 0, car, pos.X, pos.Y, pos.Z, MaxSpeed, 0, car.Model.Hash, DrivingStyle, 14f, 13f);
                                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, car, 1, 3000);
                                    TempSequence.Close();
                                    driver.Task.PerformSequence(TempSequence);
                                    TempSequence.Dispose();
                                    

                                    //Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, driver, car, pos.X, pos.Y, pos.Z, speed, DrivingStyle, 15f);
                                }
                                else
                                {
                                    StreetRaces.RaceWaypointBlips[CurrentWaypoint].Scale = 1f;
                                    StreetRaces.RaceWaypointBlips[CurrentWaypoint].IsShortRange = false;
                                    if (CurrentCheckpoint > 0)
                                    {
                                        StreetRaces.RaceWaypointBlips[CurrentWaypoint - 1].Scale = 0.7f;
                                        StreetRaces.RaceWaypointBlips[CurrentWaypoint - 1].IsShortRange = true;

                                    }
                                    else
                                    {
                                        StreetRaces.RaceWaypointBlips[StreetRaces.RaceWaypointBlips.Count - 1].Scale = 0.7f;
                                        StreetRaces.RaceWaypointBlips[StreetRaces.RaceWaypointBlips.Count - 1].IsShortRange = true;

                                    }
                                }
                            }
                        }
                        //if(driver.IsPlayer) Util.FlareUpNextCheckpoint(StreetRaces.RaceWaypoints[CurrentWaypoint]);
                    }
                }
            }
            else
            {
                if (!OutOfRace)
                {
                    car.LeftIndicatorLightOn = true;
                    car.RightIndicatorLightOn = true;
                    Util.WarnPlayer(StreetRaces.ScriptName, "RACER OUT", Name + " is out of the race.");
                    StreetRaces.RacersFinishedTreshold--;
                    OutOfRace = true;
                }

            }
            DistanceToWaypoint = (float)Math.Round(car.Position.DistanceTo(StreetRaces.RaceWaypoints[CurrentWaypoint]));

            if (driver.IsPlayer)
            {
                StreetRaces.RaceWaypointBlips[CurrentCheckpoint].Scale = 1f;
                shouldnotify = false;
            }

            
            if (shouldnotify && GameplayCamera.Position.DistanceTo(Game.Player.Character.Position)<10f)
            {
               Util.DisplayHelpTextThisFrame(notification);
            }
            else if (driver.IsPlayer && !driver.IsOnFoot && !StreetRaces.Debug || (StreetRaces.BetFromPlayer != null && StreetRaces.BetFromPlayer == this)) Util.DisplayHelpTextThisFrame("Lap: " + CurrentLap + "/" + StreetRaces.Laps + " | Waypoint: " + CurrentWaypoint + " | Pos: " + PositionInRace);
            
        }
      public  bool LetOffTheGas = false;
        void HandleBrakes()
        {

            if (HoldBrakeTime > Game.GameTime + 4000) HoldBrakeTime= 0;
            if (HoldBrakeTime < Game.GameTime)
            {
                if(car.Acceleration>0 && BrakeTargetspeed > 0)
                {
                    driver.DrivingSpeed =(float) Math.Round(BrakeTargetspeed,1);
                    if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify("~y~"+car.FriendlyName+" is not braking, fixing");
                }
                if (LetOffTheGas || ((BrakeTargetspeed!=0 && car.Speed-BrakeTargetspeed>40f) || BrakeTargetspeed!=0 && BrakeTargetspeed + 1f >= Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y))
                {
                    if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + " ~g~releases~w~ dynamic brake ("+car.Speed+")");
                    BrakeTargetspeed = 0;
                    driver.DrivingSpeed = 200f;
                    

                    if (Handbrake)
                    {
                        car.HandbrakeOn = false;
                        Handbrake = false;
                    }
                    LetOffTheGas = false;

                    //  HoldBrakeTime = Game.GameTime + 500;
                    return;
                }                
            }

            if (HoldBrakeTime < Game.GameTime && BrakeTargetspeed<3f)//
            {
                bool ShouldBrake = false;
                if (!Offroading)
                {

                    if(!ShouldBrake && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y > 20f && Util.SlidingTreshold(car, 10))
                    {
                        if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + "let go; high speed drift detected");
                        BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - 10f;
                        ShouldBrake = true;
                        HoldBrakeTime = Game.GameTime + 1000;
                        LetOffTheGas = true;
                    }
                    if (!ShouldBrake && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y > 20f && Math.Abs(car.SteeringAngle) > 35f)
                    {
                        if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + " let go; steer too high");
                        BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - 2f;
                        ShouldBrake = true;
                        HoldBrakeTime = Game.GameTime + 1000;
                        LetOffTheGas = true;
                    }

                    if (!ShouldBrake && StreetRaces.AISpinout.Checked)
                    {
                        
                        if (!Util.IsStable(car, 20f, 1f) && Util.IsPointOnRoad(car.Position, car) && Util.ForwardSpeed(car) > 20f)
                        {
                            if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + "let go; spinout detected");
                            BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - 1f; // BrakeTime += 500;
                            ShouldBrake = true;
                            HoldBrakeTime = Game.GameTime + 1000;
                            LetOffTheGas = true;
                        }
                        Util.HandleAISpinoutHelp(car);
                    }
                }

                if (NitroSafety > 40 && Util.IsRoadBusy(car.Position + car.Velocity, 8)) NitroSafety = 30 ;
                if (!ShouldBrake)
                {
                    if (!ShouldBrake && car.Speed > 60f)
                    {
                        Vector3 target = car.Position + car.Velocity;
                        if (Util.RoadHasFlag(target, Util.PathnodeFlags.Freeway) && Util.IsRoadBusy(target, 8))
                        {
                            if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + "brake; Freeway is busy");
                            ShouldBrake = true;
                            BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y * 0.8f;
                            if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y > 65f)
                            {
                                LetOffTheGas = false;
                            }
                            else
                            {
                                HoldBrakeTime = Game.GameTime + 2000;
                                LetOffTheGas = true;
                            }
                        }
                    }
                    if (!ShouldBrake && car.Speed > 45f)
                    {
                        Vector3 target = car.Position + car.Velocity;
                        if(!Util.RoadHasFlag(target, Util.PathnodeFlags.Freeway) && Util.IsRoadBusy(target, 7))
                        {
                            if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + "brake; Road is busy");
                            ShouldBrake = true;
                            BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y * 0.8f;
                            if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y > 50f)
                            {
                                LetOffTheGas = false;
                            }
                            else
                            {
                                HoldBrakeTime = Game.GameTime + 2000;
                                LetOffTheGas = true;
                            }
                        }
                    }
                }




                if (!ShouldBrake && StreetRaces.AICurvePathing.Checked && Util.HandleAIOutOfPath(car, PositionInRace > 2, Offroading, Flat))
                {
                    BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - 5f;
                    //if(Game.Player.Character.IsInRangeOf(car.Position, 5f)) UI.Notify("Shouldbrake");
                    //BrakeTime += 2000;   
                    ShouldBrake = true;
                    HoldBrakeTime = Game.GameTime + 1000;
                    LetOffTheGas = true;
                }
                if (!ShouldBrake && StreetRaces.AIRearEnd.Checked)
                {
                    //if (Game.Player.Character.IsInRangeOf(car.Position, 5f)) UI.Notify("Shouldbrake");
                    float b = Util.HandleAIRearEndHelpSP(this);
                    if (b > 0 && b < Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y)
                    {
                        BrakeTargetspeed = b;
                        ShouldBrake = true;
                        if (Util.GetRandomInt(0, 10) < 4) Function.Call<Vector3>(Hash.START_VEHICLE_HORN, car, 2000, Game.GenerateHash("NORMAL"), false);
                    }
                }


                if (!ShouldBrake && StreetRaces.AISlamToWall.Checked && Util.HandleAISlamToWall(car))
                {
                    //Handbrake = true;
                    //  if (Game.Player.Character.IsInRangeOf(car.Position, 5f)) UI.Notify("Shouldbrake");
                    BrakeTargetspeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - 5f;
                    LetOffTheGas = true;
                    //BrakeTime += 500;
                    ShouldBrake = true;
                }



                if (ShouldBrake)
                {

                    if (!LetOffTheGas && (car.Speed < BrakeTargetspeed || BrakeTargetspeed<5f ))
                    {
                        BrakeTargetspeed = 0;
                        HoldBrakeTime =0;
                        if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify("~r~Brake cancelled.");
                        return;
                    }
                    if (NitroSafety > 0) NitroSafety = 0;
                    if (BrakeTargetspeed < 5f) BrakeTargetspeed = 5f;

                    //  if (Game.Player.Character.IsInRangeOf(car.Position, 5f)) UI.Notify("~r~Shouldbrake");
                    if (BrakeTargetspeed  < Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y)
                    {
                        if (BrakeTargetspeed < Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - 25f) Handbrake = true;

                        if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify(car.FriendlyName + "  ~o~uses~w~ dynamic brake  (" + Math.Round(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y,0 )+ " > " + Math.Round( BrakeTargetspeed,0)+ ")");



                        if (!LetOffTheGas)
                        {
                            driver.DrivingSpeed = (float)Math.Round(BrakeTargetspeed, 2) - 1f;
                            if (Handbrake) car.HandbrakeOn = true;
                        }

                        /*
                        if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y - BrakeTargetspeed < 10) //if speeds are similar keep the brake some more time
                        {
                            if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, car, true).Y < 30)
                            {
                                HoldBrakeTime = Game.GameTime + 1000;
                                int d = (Util.GetRandomInt(1, 2) * 1000);
                                NitroTimeLimit = Game.GameTime + d;
                                if (Game.Player.Character.IsSittingInVehicle(car)) UI.Notify("~g~Nitro time limit updated, +" + d / 1000 + "s");

                            }
                            else
                            {
                               // HoldBrakeTime = Game.GameTime + 500;
                            }
                        }
                        */
                    }
                    else
                    {
                        BrakeTargetspeed = 0;
                        HoldBrakeTime = 0;

                        if (StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(car)) UI.Notify("~r~Brake cancelled.");
                        return;
                    }
                    //else BrakeTargetspeed = 0;
                }
            }
        }
    }
}