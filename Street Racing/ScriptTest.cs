using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using NativeUI;
using System.Linq;

namespace Street_Races
{
    public enum RacePhase
    {
        NotRacing,
        RaceSetup,
        RacePreps,
        RaceCountDown,
        RaceInProgress,
        RaceEnded
    }

    public enum Raceclass
    {
        Tuner,
        Street,
        Super,
        Sponsored,
        Offroad,
    }

    public enum Tuneconfig
    {
        All,
        NonTuned,
        None,
    }

    /// <summary>
    ///  TO DO
    /// 1. In the settings put (immortality ped)
    ///2. In the settings put(do not kill the car)
    ///3. increase the time before the start of 1 min(or after the start of the team) (if applicable) (I need to have time to manually arrange car) (or how to do it)
    ///4. Correct the errors in the version 0.7 (not work-colored cars(cars addon), and there are anomalies) (so I'm using v0.6)
    /// - RACE INTRO?
    /// - ADD IMMERSIVE WAYPOINTS - SEMI DONE
    /// - ADD HUD
    /// - ADD PERSONALITY TO DRIVERS
    /// - ADD LAMAR CONTEXTUAL MESSAGES
    /// - ADD AI OVERTAKE INITIATIVE
    /// </summary>
    /// 


    public class StreetRaces : Script
    {
        public static string ScriptName = "Street Racing";
        public static string ScriptVer = "0.7";
        public static string ScriptFolder = "StreetRaces";

        public static bool Debug = false;
        public static bool ShouldForceClean = false;


        //Triggers info
        Dictionary<Vector3, string> PosToRace = new Dictionary<Vector3, string>();
        public static List<Vector3> RaceTriggers = new List<Vector3>();
        public static List<Blip> RaceTriggersBlips = new List<Blip>();

        //Race Info
        public static int MaxRacers = 5;
        public static Racer Winner;
        public static List<Vector3> RaceWaypoints = new List<Vector3>();
        public static List<Vector3> RaceDangerousWaypoints = new List<Vector3>();
        public static List<Vector3> RaceOffroadWaypoints = new List<Vector3>();
        public static List<Vector3> RaceFlatWaypoints = new List<Vector3>();

        public static List<Blip> RaceWaypointBlips = new List<Blip>();
        public static int RaceHeading = 0;
        public static List<Racer> Racers = new List<Racer>();
        public static RacePhase RaceStatus = RacePhase.NotRacing;
        public static int Laps = 1;
        public static Raceclass RaceClass = Raceclass.Tuner;
        public static List<dynamic> RaceClasses = new List<dynamic>();

        public static int RacersFinished = 0;
        public static int RacersFinishedTreshold = 0;
        public static int FirstRacerFinishTime = 0;
        public static int FinishRaceCountdown = 5000; //30000
        public int MainGameTimeRef = Game.GameTime;
        public int MainGameTimeRefInterval = 300;
        public static List<Prop> Props = new List<Prop>();
        public static List<string> DriverModels = new List<string> { "U_M_Y_CYCLIST_01","hc_hacker","a_m_m_skidrow_01","a_m_y_ktown_01","a_m_y_latino_01","a_m_y_motox_01", "a_m_y_motox_02","a_m_y_roadcyc_01" , "a_m_y_runner_02" };
        public static Racer BetFromPlayer = null;


        //Temporal car classes
        public static List<VehicleHash> Sports = new List<VehicleHash> { VehicleHash.Comet2, VehicleHash.Buffalo, VehicleHash.Kuruma, VehicleHash.Sultan, VehicleHash.Tampa, VehicleHash.Jester, VehicleHash.Furoregt };
        public static List<VehicleHash> Super = new List<VehicleHash> { VehicleHash.Adder, VehicleHash.Infernus, VehicleHash.Zentorno, VehicleHash.Voltic, VehicleHash.Bullet, VehicleHash.Osiris };
        public static List<VehicleHash> Classic = new List<VehicleHash> { VehicleHash.Mamba, VehicleHash.Coquette3, VehicleHash.Pigalle, VehicleHash.StingerGT, VehicleHash.Casco };

        //Heli
        public static Vehicle Heli;
        public static Ped HeliPilot;


        public static List<dynamic> ClassList = new List<dynamic>();
        public static List<dynamic> NumberOfRacersList = new List<dynamic> { 1, 2, 4, 6, 12, 16, 32, 64 };
        public static List<dynamic> LapsList = new List<dynamic> { 1, 2, 3, 4, 5, 10, 15, 20, 50, 100 };

        public static List<dynamic> MoneyBetList = new List<dynamic> { "Free", "~g~$~w~" + 200, "~g~$~w~" + 500, "~g~$~w~" + "1.000", "~g~$~w~" + "2.000", "~g~$~w~" + "5.000", "~g~$~w~" + "10.000", "~g~$~w~" + "50.000", "~g~$~w~" + "100.000" };
        public static List<dynamic> RealMoneyBetList = new List<dynamic> { 0, 200, 500, 1000, 2000, 5000, 10000, 50000, 100000 };

        public static int Prize = 0;
        public static int Bet = 0;

        private MenuPool _menuPool;


        //Race menu


        public static List<dynamic> TuneconfigListText = new List<dynamic> { "All cars", "Non-tuned", "None" };
        public static List<dynamic> TuneconfigList = new List<dynamic> { 0, 1, 2 };


        private UIMenu RaceSetupMenu;
        private UIMenu AdvancedOptionsMenu;
        private UIMenuItem AdvancedOptions;

        private UIMenuItem StartRace;
        public static UIMenuListItem CarClass;
        public static UIMenuListItem SetupCarsNumber;
        public static UIMenuListItem SetupLaps;
        public static UIMenuListItem MoneyBet;

        //public static UIMenuCheckboxItem ForceMyCar = new UIMenuCheckboxItem("Force Current Car",true,"Forces all racers to drive the same car as you.");

        public static UIMenuCheckboxItem DebugMode = new UIMenuCheckboxItem("Debug Info", false, "For developer purposes, or if you want to know how this script works.");
        public static UIMenuCheckboxItem Traffic = new UIMenuCheckboxItem("Traffic", true, "Toggles traffic for the race.");

        public static UIMenuCheckboxItem UseNitro = new UIMenuCheckboxItem("AI Can use Nitro", true, "Allows Nitro usage for the AI. You'll have to provide your own nitro (with a mod) for now.");


        public static UIMenuCheckboxItem AICatchup = new UIMenuCheckboxItem("AI Catchup System", true, "Makes races more interesting by helping the AI who has fallen back too far.");
        public static UIMenuCheckboxItem AICurvePathing = new UIMenuCheckboxItem("AI Curve Help System", true, "Helps the AI around corners.");
        public static UIMenuCheckboxItem AIRearEnd = new UIMenuCheckboxItem("AI Anti-RearEnd System", true, "Forces the AI to brake before bumping into someone. Helps them race more cleanly.");
        public static UIMenuCheckboxItem AISpinout = new UIMenuCheckboxItem("AI Anti-SpinOut System", true, "Helps the AI keep control of the car in tricky situations.");
        public static UIMenuCheckboxItem AISlamToWall = new UIMenuCheckboxItem("AI Anti-Crash System", true, "Forces the AI to brake before crashing into a wall.");

        public static UIMenuListItem ForceTuneconfig;

        public static UIMenuCheckboxItem ImmersiveCheckpoints = new UIMenuCheckboxItem("Immersive Checkpoint display", false, "Implements a less immersion-breaking checkpoint display. Still in beta.");

        //public static UIMenuCheckboxItem ForceTune = new UIMenuCheckboxItem("Auto-tuning for all cars", true, "Forces random tuning for every racer. Adds variety to the races, but the auto-tuner doesn't have much sense of style.");
        public static UIMenuCheckboxItem AllowFilling = new UIMenuCheckboxItem("Fill with random opponents", false, "If you select a Nº opponents superior to the number of opponents saved in Racers.xml, the script will fill the race with random opponents, using previously spawned cars.");
        public static UIMenuCheckboxItem AllowHelicopter = new UIMenuCheckboxItem("Enable Helicopter", false, "Spawns an helicopter that follows the leader.");
        public static UIMenuCheckboxItem AIGodmode = new UIMenuCheckboxItem("AI Godmode", true, "Forces godmode for the AI");

        public StreetRaces()
        {

            Game.FadeScreenIn(500);
            Util.GetClasses();


            ClassList = RaceClasses; //Enum.GetValues(typeof(Raceclass)).Cast<dynamic>().ToList();
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            LoadRaceTriggers();

            _menuPool = new MenuPool();

            //Car Saver Menu
            RaceSetupMenu = new UIMenu("Race Setup", "Change your preferences before the race.");
            _menuPool.Add(RaceSetupMenu);

            AdvancedOptionsMenu = new UIMenu("Adv. Options", "For those who know what they're doing.");
            _menuPool.Add(AdvancedOptionsMenu);



            AdvancedOptions = new UIMenuItem("Advanced Options", "");
            AdvancedOptions.SetLeftBadge(UIMenuItem.BadgeStyle.Michael);
            RaceSetupMenu.AddItem(AdvancedOptions);
            RaceSetupMenu.BindMenuToItem(AdvancedOptionsMenu, AdvancedOptions);


            CarClass = new UIMenuListItem("Vehicle Class", ClassList, 0);
            RaceSetupMenu.AddItem(CarClass);

            SetupCarsNumber = new UIMenuListItem("Nº of Racers", NumberOfRacersList, 0);
            RaceSetupMenu.AddItem(SetupCarsNumber);


            SetupLaps = new UIMenuListItem("Nº of Laps", LapsList, 0);
            RaceSetupMenu.AddItem(SetupLaps);


            MoneyBet = new UIMenuListItem("Bet", MoneyBetList, 0, "Each racer bets this ammount of money. The winner gets it all.");
            RaceSetupMenu.AddItem(MoneyBet);

            RaceSetupMenu.AddItem(UseNitro);
            RaceSetupMenu.AddItem(AllowHelicopter);
            RaceSetupMenu.AddItem(AIGodmode);
            RaceSetupMenu.AddItem(AllowFilling);          
            ForceTuneconfig = new UIMenuListItem("Auto Opponent Tuning", TuneconfigListText, 0, "Opponent cars can be randomly tuned for more variety.");


            AdvancedOptionsMenu.AddItem(ForceTuneconfig); //Test
            AdvancedOptionsMenu.AddItem(ImmersiveCheckpoints);
            AdvancedOptionsMenu.AddItem(DebugMode);
            AdvancedOptionsMenu.AddItem(Traffic);
            AdvancedOptionsMenu.AddItem(AICatchup);
            AdvancedOptionsMenu.AddItem(AICurvePathing);
            AdvancedOptionsMenu.AddItem(AIRearEnd);
            AdvancedOptionsMenu.AddItem(AISlamToWall);
            AdvancedOptionsMenu.AddItem(AISpinout);

            StartRace = new UIMenuItem("Start Race", "");
            RaceSetupMenu.AddItem(StartRace);



            foreach (UIMenu menu in _menuPool.ToList())
            {
                menu.RefreshIndex();
                menu.OnItemSelect += OnItemSelect;
                menu.OnListChange += OnListChange;
                menu.OnCheckboxChange += OnCheckboxChange;
                /* menu.OnIndexChange += OnItemChange;
                 menu.OnMenuClose += OnMenuClose;*/
            }
            SetupCarsNumber.Index = Util.GetRandomInt(0, NumberOfRacersList.Count / 2);
            SetupLaps.Index = Util.GetRandomInt(0, LapsList.Count / 2);
        }

        public void OnListChange(UIMenu sender, UIMenuListItem list, int index)
        {
            //if(list == SetupCarsNumber) MoneyBetDesc = "Each racer bets this ammount of money. The winner gets it all.";
        }
        public void OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkbox, bool Checked)
        {
            if (checkbox == DebugMode) Debug = DebugMode.Checked;
        }
        public void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == StartRace)
            {
                if (Game.Player.Money >= RealMoneyBetList[MoneyBet.Index])
                {
                    foreach (Vector3 waypoint in RaceWaypoints)
                    {
                        Blip blip = World.CreateBlip(waypoint);
                        blip.IsShortRange = true;
                        blip.Scale = 0.6f;
                        RaceWaypointBlips.Add(blip);
                    }
                    RaceStatus = RacePhase.RacePreps;

                    foreach (Blip blip in RaceTriggersBlips) blip.Alpha = 0;

                    _menuPool.CloseAllMenus();
                    if (RealMoneyBetList[MoneyBet.Index] != 0) Game.Player.Money -= RealMoneyBetList[MoneyBet.Index];
                    Prize = (RealMoneyBetList[MoneyBet.Index] * (SetupCarsNumber.IndexToItem(SetupCarsNumber.Index) + 1));
                    Bet = RealMoneyBetList[MoneyBet.Index];
                    Script.Wait(1000);
                }
                else
                {
                    UI.Notify("~r~You don't have enough money join this race.");
                }

            }
        }
        void LoadRaceTriggers()
        {
            foreach (Blip blip in RaceTriggersBlips) blip.Remove();
            RaceTriggersBlips.Clear();
            RaceTriggers.Clear();

            int Races = 0;
            foreach (string file in Directory.GetFiles(@"scripts\" + ScriptFolder, "*.xml"))
            {
                XmlDocument originalXml = new XmlDocument();
                originalXml.Load(file);
                Vector3 pos = Vector3.Zero;

                if (originalXml.SelectSingleNode("//Meet") != null)
                {
                    Races++;
                    pos = new Vector3(float.Parse(originalXml.SelectSingleNode("//Meet/X").InnerText), float.Parse(originalXml.SelectSingleNode("//Meet/Y").InnerText), float.Parse(originalXml.SelectSingleNode("//Meet/Z").InnerText));
                }
                else
                {
                    if (originalXml.SelectSingleNode("//Waypoint") != null)
                    {
                        Races++;
                        pos = new Vector3(float.Parse(originalXml.SelectSingleNode("//Waypoint/X").InnerText), float.Parse(originalXml.SelectSingleNode("//Waypoint/Y").InnerText), float.Parse(originalXml.SelectSingleNode("//Waypoint/Z").InnerText));
                    }
                    else
                    {
                        Util.WarnPlayer(ScriptName, "FILE NOT FOUND", "The relevant files for this script cannot be found. Please, reinstall.");
                        break;
                    }
                }


                XmlElement element = originalXml.DocumentElement;
                string name = element.GetAttribute("Race_Name");
                PosToRace.Add(pos, name);
                RaceTriggers.Add(pos);

                Blip blip = World.CreateBlip(pos);
                blip.IsShortRange = true;
                blip.Sprite = BlipSprite.RaceCar;
                //blip.Scale = 2.0f;
                blip.Color = BlipColor.White;

                RaceTriggersBlips.Add(blip);

                Function.Call(Hash._0xF9113A30DE5C6670, "STRING");
                Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, "Street Race");
                Function.Call(Hash._0xBC38B49BCB83BC9B, blip);
                if (Debug) UI.Notify(name + " ~g~loaded.");

            }
            Util.WarnPlayer(ScriptName, " ", Races + " races loaded.");

        }

        void DrawTriggers()
        {
            foreach (Vector3 pos in RaceTriggers)
            {
                if (Game.Player.Character.Position.DistanceTo(pos) < 40f)
                {
                    World.DrawMarker(MarkerType.CheckeredFlagRect, pos + new Vector3(0, 0, 4), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(3, 3, 3), Color.White, true, true, 1, false, null, null, false);
                    if (Game.Player.Character.Position.DistanceTo(pos) < 25f)
                    {
                        if (Game.Player.Character.IsOnFoot)
                        {
                            DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to start this race as a Spectator.");
                        }
                        else
                        {
                            DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to start this race.");
                        }
                    }
                }
            }
        }


        void DrawWaypoints()
        {

            /*
            foreach (Prop prop in Props)
            {
                int alpha = 255;
                if (Game.Player.Character.Position.DistanceTo(prop.Position) < alpha) alpha = (int)Game.Player.Character.Position.DistanceTo(prop.Position);
                World.DrawMarker(MarkerType.VerticalCylinder, prop.Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 50), Color.FromArgb(alpha, 69,65,150), false, true, 0, false, "", "", false);
            }*/

            if (Util.IsPlayerParticipating())
            {
                for (int i = 0; i < RaceWaypoints.Count - 1; i++)
                {
                    if (Util.GetPlayerRacer().CurrentCheckpoint == RaceWaypoints.Count - 1)
                    {
                        if (Util.GetPlayerRacer().CurrentLap == Laps) World.DrawMarker(MarkerType.CheckeredFlagRect, RaceWaypoints[0] + new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(3, 3, 3), Color.White, false, true, 0, false, "", "", false);
                        else
                        {
                            if (ImmersiveCheckpoints.Checked)
                            {
                                int alpha = 255;
                                if (Game.Player.Character.Position.DistanceTo(Props[0].Position) < alpha) alpha = (int)Game.Player.Character.Position.DistanceTo(Props[0].Position) - 50;
                                if (alpha < 0) alpha = 0;
                                World.DrawMarker(MarkerType.VerticalCylinder, Props[0].Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 50), Color.FromArgb(alpha, 16, 68, 181), false, true, 0, false, "", "", false);
                            }
                            else
                            {
                                World.DrawMarker(MarkerType.VerticalCylinder, RaceWaypoints[0] + new Vector3(0, 0, -5), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(10, 10, 6), Color.Yellow);
                            }
                        }
                        return;
                    }
                    else if (i == Util.GetPlayerRacer().CurrentCheckpoint)
                    {
                        if (ImmersiveCheckpoints.Checked)
                        {
                            int alpha = 255;
                            if (Game.Player.Character.Position.DistanceTo(Props[i + 1].Position) < alpha) alpha = (int)Game.Player.Character.Position.DistanceTo(Props[i + 1].Position) - 50;
                            if (alpha < 0) alpha = 0;
                            World.DrawMarker(MarkerType.VerticalCylinder, Props[i + 1].Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 50), Color.FromArgb(alpha, 16, 68, 181), false, true, 0, false, "", "", false);
                        }
                        else
                        {
                            World.DrawMarker(MarkerType.VerticalCylinder, RaceWaypoints[i + 1] + new Vector3(0, 0, -5), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(10, 10, 6), Color.Yellow);
                        }
                        return;
                    }
                }
            }

        }

        void HandlePositions()
        {
            Laps = SetupLaps.IndexToItem(SetupLaps.Index);

            string text = " ";
            int CurrentPositionChecked = 1 + RacersFinished;
            int CheckpointsChecked = 0;
            int NumberOfCheckpoints = RaceWaypoints.Count;

            foreach (Racer racer in Racers)
            {
                if (!racer.driver.IsPlayer && racer.car.CurrentBlip != null)
                {
                    racer.car.CurrentBlip.HideNumber();
                }
            }

            //For each lap, from the last to the first
            for (int lap = Laps; lap >= 0; lap--)
            {
                //For each waypoint, from the last to the first
                for (int number = NumberOfCheckpoints; number >= 0; number--)
                {

                    List<float> DistanceToWaypoint = new List<float>();
                    List<Racer> CompetitorsThisWaypoint = new List<Racer>();


                    foreach (Racer racer in Racers)
                    {
                        //if (racer.CurrentWaypoint == 0) racer.CurrentWaypoint = NumberOfCheckpoints;
                        if (racer.CurrentLap == lap && racer.CurrentCheckpoint == number)
                        {
                            DistanceToWaypoint.Add(racer.DistanceToWaypoint);
                            CompetitorsThisWaypoint.Add(racer);
                        }
                    }

                    DistanceToWaypoint.Sort();
                    foreach (float dist in DistanceToWaypoint)
                    {
                        foreach (Racer racer in CompetitorsThisWaypoint)
                        {
                            if (dist == racer.DistanceToWaypoint)
                            {
                                racer.PositionInRace = CurrentPositionChecked;
                                racer.car.CurrentBlip.ShowNumber(CurrentPositionChecked);
                                CurrentPositionChecked++;
                            }
                        }
                    }
                }
            }
            //DisplayHelpTextThisFrame(text);

        }

        public void CleanRace()
        {
            Winner = null;
            FirstRacerFinishTime = 0;
            foreach (Racer racer in Racers) racer.Delete();
            foreach (Blip blip in RaceWaypointBlips) blip.Remove();
            foreach (Prop prop in Props) prop.Delete();


            if (CanWeUse(HeliPilot)) HeliPilot.IsPersistent = false;
            if (CanWeUse(Heli)) Heli.IsPersistent = false;
            //if(CanWeUse(Heli) && CanWeUse(HeliPilot)) Function.Call(Hash.TASK_HELI_MISSION, HeliPilot, Heli, 0, 0, Heli.Position.X, Heli.Position.Y, Heli.Position.Z, 20, 60f, 40f, 270f, 0f, 30, 50f, 0);

            Props.Clear();

            Racers.Clear();
            RaceWaypoints.Clear();
            RaceWaypointBlips.Clear();
            RaceDangerousWaypoints.Clear();
            RaceStatus = RacePhase.NotRacing;
            PrepsStatus = PrepsPhase.Nothing;
            RacersFinished = 0;
            foreach (Blip blip in RaceTriggersBlips) blip.Alpha = 255;

        }
        public static bool WasCheatStringJustEntered(string cheat)
        {
            return Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash(cheat));
        }
        void OnTick(object sender, EventArgs e)
        {
           //Util.GetRoadFlags(Game.Player.Character.Position);

            if (WasCheatStringJustEntered("st")) BaseDrivingStyle = int.Parse( Game.GetUserInput(10));
            //Util.DisplayHelpTextThisFrame(Util.IsPotentiallySliding(Game.Player.Character.CurrentVehicle, 0.3f).ToString());
            //Test
            /*
            Vector3 posit = Game.Player.Character.Position + (Game.Player.Character.ForwardVector * 10);
            RaycastResult raycast = World.Raycast(Game.Player.Character.Position , posit, IntersectOptions.Everything);
            if(raycast.DitHitAnything)
            {
                Util.DisplayHelpTextThisFrame(Math.Round(raycast.SurfaceNormal.Z,3).ToString());

                World.DrawMarker(MarkerType.ThinChevronUp, raycast.HitCoords, raycast.SurfaceNormal, new Vector3(0, 0, 0), new Vector3(2, 2, 2), Color.Red);

                Util.DrawLine(Game.Player.Character.Position, raycast.HitCoords);
            }*/

            HandleHeli();
            //Util.HandleAISpinoutHelp(Game.Player.Character.CurrentVehicle);


            //DisplayHelpTextThisFrame(Game.Player.Character.CurrentVehicle.Acceleration.ToString());
            _menuPool.ProcessMenus();
            switch (RaceStatus)
            {
                case RacePhase.NotRacing:
                    {
                        DrawTriggers();
                        break;
                    }
                case RacePhase.RaceSetup:
                    {
                        break;
                    }
                case RacePhase.RacePreps:
                    {
                        if (!Traffic.Checked)
                        {
                            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0.0f);
                            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
                            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
                        }
                        if (HandleRacePreps())
                        {
                            if (!Util.IsPlayerParticipating())
                            {
                                Util.DisplayHelpTextThisFrame("Bet on any of the drivers approaching their car.");
                                foreach (Racer racer in Racers)
                                {
                                    if (Game.Player.Character.Position.DistanceTo(racer.car.Position) < 3f)
                                    {
                                        Util.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to bet " + Bet + "~g~$~w~ on the "+racer.Name+".");

                                        if (Game.IsControlJustReleased(2, GTA.Control.Context))
                                        {
                                            Game.Player.Character.Task.EnterVehicle(racer.car, VehicleSeat.Passenger, 3000);
                                            BetFromPlayer = racer;
                                            RaceStatus = RacePhase.RaceCountDown;
                                            break;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                RaceStatus = RacePhase.RaceCountDown;
                            }
                        }
                        break;
                    }
                case RacePhase.RaceCountDown:
                    {
                        if (!Traffic.Checked)
                        {
                            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0.0f);
                            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
                            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
                        }
                        if (Util.Countdown())
                        {
                            if (Util.IsPlayerParticipating())
                            {
                                if (CanWeUse(Game.Player.Character.CurrentVehicle)) Game.Player.Character.CurrentVehicle.HandbrakeOn = false;

                                if (Prize > 0) Util.AddNotification("CHAR_LAMAR", "~b~Lamar", "THE PRIZE", "The winner leaves with ~g~$" + Prize + "~w~ in his pocket, guys!");
                                else Util.AddNotification("CHAR_LAMAR", "~b~Lamar", "FRIENDLY RACE", "There's no prize for winning this race, so lets keep it clean!");
                                //Util.AddNotification("CHAR_LAMAR", "~b~Lamar", "#", "Lets RACE.");

                                //Util.AddNotification("","","","The prize for this race is~g~ $"+Prize+"~w~.");
                            }
                            if (ImmersiveCheckpoints.Checked)
                            {
                                foreach (Vector3 pos in RaceWaypoints)
                                {
                                    //prop_basejump_target_01 prop_parking_wand_01 prop_air_lights_03a
                                    Prop light = World.CreateProp("prop_air_lights_03a", pos, new Vector3(0, 0, 0), true, false);
                                    Props.Add(light);
                                }
                            }
                            if (RacersFinishedTreshold > Racers.Count) RacersFinishedTreshold = Racers.Count;
                            RaceStatus = RacePhase.RaceInProgress;
                        }
                        else
                        {
                            if (Util.IsPlayerParticipating() && CanWeUse(Game.Player.Character.CurrentVehicle)) Game.Player.Character.CurrentVehicle.HandbrakeOn = true;
                        }
                        break;
                    }
                case RacePhase.RaceInProgress:
                    {
                        if (RacersFinished > 0 && FirstRacerFinishTime == 0)
                        {
                            FirstRacerFinishTime = Game.GameTime + FinishRaceCountdown;
                            Util.AddNotification("CHAR_LAMAR", "~b~Lamar", "First racer finished", FinishRaceCountdown / 1000 + " seconds until the race ends!");
                        }
                        if (Winner != null && RacersFinished > 0 && FirstRacerFinishTime < Game.GameTime)
                        {
                            if (Prize > 0) Util.AddNotification("CHAR_LAMAR", "~b~Lamar", "Race ended", "~b~" + Winner.Name + "~w~ gets the " + Prize + "~g~$~w~ prize!");

                            if (Winner.driver.IsPlayer)
                            {
                                Game.Player.Money += Prize;
                                //UI.Notify("You have won!"); 
                            }
                            else
                            {
                                Winner.driver.Money = Winner.driver.Money + Prize;
                                if (Winner.car.FriendlyName == Winner.Name) UI.Notify("The ~b~" + Winner.car.FriendlyName + "~w~ has won."); else UI.Notify("~b~" + Winner.Name + "~w~ has won.");
                                if (BetFromPlayer != null && BetFromPlayer == Winner)
                                {
                                    UI.Notify("~g~The car you betted on has won! You have won " + Bet + "~g~$~w~.");
                                    Game.Player.Money += Bet;
                                }
                            }
                            CleanRace();
                        }
                        if (ImmersiveCheckpoints.Checked) foreach (Prop prop in Props) if (prop.HeightAboveGround > 0.3f) prop.ApplyForce(new Vector3(0, 0, -1));
                        DrawWaypoints();

                        HandlePositions();
                        if (!Traffic.Checked)
                        {
                            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0.0f);
                            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
                            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
                        }
                        if (Game.Player.IsDead)
                        {
                            RaceStatus = RacePhase.NotRacing;
                            //UI.Notify("");
                            DisplayHelpTextThisFrame("~r~Race Cancelled.");
                            CleanRace();
                        }
                        if (Util.IsPlayerParticipating())
                        {

                            if (Game.Player.Character.IsOnFoot && RacersFinished == 0)
                            {
                                DisplayHelpTextThisFrame("Get back to your car.");

                                if (Game.Player.Character.Position.DistanceTo(Util.GetPlayerRacer().car.Position) > 15f)
                                {
                                    RaceStatus = RacePhase.NotRacing;
                                    //UI.Notify("");
                                    DisplayHelpTextThisFrame("~r~Race Cancelled.");
                                    CleanRace();
                                }
                            }
                        }

                        break;
                    }
                case RacePhase.RaceEnded:
                    {
                        UI.Notify("~g~Race finished.");
                        break;
                    }
            }

            //Process Racer AI
            int QuickRaceTime = 0;//Game.GameTime + 300;
            if (1==1)
            {
                foreach (Racer racer in Racers) racer.Tick();

                if (MainGameTimeRef < Game.GameTime)
                {
                    foreach (Racer racer in Racers) racer.ProcessRace();
                    MainGameTimeRef = Game.GameTime + 100;
                }
            }
            Util.HandleNotifications();

            if (Game.IsControlJustPressed(2, GTA.Control.Context))
            {
                switch (RaceStatus)
                {
                    case RacePhase.NotRacing:
                        {
                            if (IsPlayerNearAnyRace())
                            {
                                if (!_menuPool.IsAnyMenuOpen())
                                {
                                    LoadRaceInfo();
                                    List<VehicleHash> Models = new List<VehicleHash>();
                                    //RaceStatus = RacePhase.RaceSetup;
                                    RaceSetupMenu.Visible = true;
                                }
                            }
                            else
                            {
                                //if (Game.Player.Character.Velocity.Length() == 0) { LoadRaceTriggers(); }
                            }
                            break;
                        }
                    case RacePhase.RaceSetup:
                        {

                            break;
                        }
                    case RacePhase.RaceCountDown:
                        {
                            break;
                        }

                    case RacePhase.RaceInProgress:
                        {

                            /*RaceStatus = RacePhase.NotRacing;
                            UI.Notify("Race ended by player input.");
                            CleanRace();
                            */

                            //Game.Player.Character.SetIntoVehicle(Racers[Util.GetRandomInt(0,Racers.Count)].car, VehicleSeat.Passenger);

                            break;
                        }
                }
            }
        }
        void OnKeyDown(object sender, KeyEventArgs e)
        {

        }
        void OnKeyUp(object sender, KeyEventArgs e)
        {
        }


        void GenerateRacers()
        {


        }
        bool IsPlayerNearAnyRace()
        {
            foreach (Vector3 pos in RaceTriggers)
            {
                if (Game.Player.Character.Position.DistanceTo(pos) < 30)
                {
                    foreach (string file in Directory.GetFiles(@"scripts\" + ScriptFolder, "*.xml"))
                    {
                        XmlDocument originalXml = new XmlDocument();
                        originalXml.Load(file);

                        if (originalXml.SelectSingleNode("//Waypoint") != null)
                        {
                            XmlElement element = originalXml.DocumentElement;
                            string name = element.GetAttribute("Race_Name");
                            if (PosToRace[pos] == name)
                            {
                                //UI.Notify("~g~Valid race.");
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        void LoadRaceInfo()
        {

            RaceWaypoints.Clear();
            RaceDangerousWaypoints.Clear();
            RaceOffroadWaypoints.Clear();
            RaceFlatWaypoints.Clear();

            bool foundfile = false;
            foreach (Vector3 pos in RaceTriggers)
            {
                if (foundfile) break;
                if (Game.Player.Character.Position.DistanceTo(pos) < 30)
                {

                    foreach (string file in Directory.GetFiles(@"scripts\" + ScriptFolder, "*.xml"))
                    {
                        if (foundfile) break;

                        XmlDocument originalXml = new XmlDocument();
                        originalXml.Load(file);

                        XmlElement element = originalXml.DocumentElement;
                        string name = element.GetAttribute("Race_Name");
                        if (name == PosToRace[pos])
                        {
                            //UI.Notify("Loading " + name);
                            int Dang = 0;
                            int Offroad = 0;
                            foreach (XmlElement waypoint in originalXml.SelectNodes("//Waypoint"))
                            {
                                Vector3 waypointpos = new Vector3(float.Parse(waypoint.SelectSingleNode("X").InnerText), float.Parse(waypoint.SelectSingleNode("Y").InnerText), float.Parse(waypoint.SelectSingleNode("Z").InnerText));
                                //UI.Notify("Loaded " + waypointpos.ToString());
                                RaceWaypoints.Add(waypointpos);
                                if (waypoint.GetAttribute("Dangerous") == "true")
                                {
                                    Dang++;
                                    RaceDangerousWaypoints.Add(waypointpos);
                                }
                                if (waypoint.GetAttribute("Offroad") == "true")
                                {
                                    RaceOffroadWaypoints.Add(waypointpos);
                                    Offroad++;
                                }
                                if (waypoint.GetAttribute("Flat") == "true")
                                {
                                    RaceFlatWaypoints.Add(waypointpos);
                                }
                            }
                            RaceHeading = int.Parse(originalXml.SelectSingleNode("//Heading").InnerText);

                            if (Debug) UI.Notify("Dangerous waypoints: " + Dang);
                            if (Debug) UI.Notify("Offroad waypoints:" + Offroad);
                            foundfile = true;
                            return;
                        }
                    }
                }
            }
        }
        public static int BaseDrivingStyle = 4 + 8 +16+32+ 512 + 262144;
        //Never used
        public void ResetEverything()
        {
            Game.FadeScreenIn(300);
            CleanRace();
            foreach (Blip blip in RaceTriggersBlips) blip.Remove();
            foreach (Blip blip in RaceWaypointBlips) blip.Remove();
            Util.WarnPlayer(StreetRaces.ScriptName + " " + ScriptVer, "SCRIPT RESTART", "~g~Script restarted successfully.");

        }
        protected override void Dispose(bool dispose)
        {
            CleanRace();
            foreach (Blip blip in RaceTriggersBlips) blip.Remove();

            foreach (Blip blip in RaceWaypointBlips) blip.Remove();
            base.Dispose(dispose);
        }

        int HeliHandleRef = Game.GameTime;
        void HandleHeli()
        {
            if (AllowHelicopter.Checked)
            {
                if (HeliHandleRef < Game.GameTime)
                {
                    HeliHandleRef = Game.GameTime + 1000;

                    if (!CanWeUse(Heli) && RaceStatus == RacePhase.RacePreps)
                    {
                        Heli = World.CreateVehicle(VehicleHash.Supervolito, Game.Player.Character.Position + new Vector3(0, 0, 200));
                        HeliPilot = World.CreateRandomPed(Game.Player.Character.Position + new Vector3(0, 0, 90));
                        HeliPilot.AlwaysKeepTask = true;
                        HeliPilot.BlockPermanentEvents = true;
                        HeliPilot.SetIntoVehicle(Heli, VehicleSeat.Driver);
                    }

                    if (CanWeUse(Heli) && CanWeUse(HeliPilot))
                    {
                        if (Racers.Count > 0)
                        {
                            if (Util.GetRacerInFirst() != null) Function.Call(Hash.TASK_HELI_MISSION, HeliPilot, Heli, 0, Util.GetRacerInFirst().driver, 0, 0, 0, 6, 60f, 40f, 270f, 0f, 30, 50f, 0);
                        }
                        else
                        {
                            if (Game.Player.Character.IsInVehicle(Heli))
                            {
                                if (Heli.Speed < 2f && Heli.HeightAboveGround > 3f)
                                {
                                    if (Heli.IsPersistent) Heli.IsPersistent = false;
                                    if (HeliPilot.IsPersistent) HeliPilot.IsPersistent = false;
                                    Function.Call(Hash.TASK_HELI_MISSION, HeliPilot, Heli, 0, Game.Player.Character, Heli.Position.X, Heli.Position.Y, Heli.Position.Z, 20, 60f, 40f, 270f, 0f, 30, 50f, 0);
                                }
                            }
                            else if (Heli.Speed < 2f)
                            {
                                Function.Call(Hash.TASK_HELI_MISSION, HeliPilot, Heli, 0, Game.Player.Character, Heli.Position.X, Heli.Position.Y, Heli.Position.Z, 8, 60f, 40f, 270f, 0f, 30, 50f, 0);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Util.CanWeUse(Heli)) Heli.IsPersistent = false;
                if (Util.CanWeUse(HeliPilot)) HeliPilot.IsPersistent = false;
            }
        }

        /// TOOLS ///
        void LoadSettings()
        {
            if (File.Exists(@"scripts\\SCRIPTNAME.ini"))
            {

                ScriptSettings config = ScriptSettings.Load(@"scripts\SCRIPTNAME.ini");
                // = config.GetValue<bool>("GENERAL_SETTINGS", "NAME", true);
            }
            else
            {
                Util.WarnPlayer(ScriptName + " " + ScriptVer, "SCRIPT RESET", "~g~Towing Service has been cleaned and reset succesfully.");
            }
        }

        bool CanWeUse(Entity entity)
        {
            return entity != null && entity.Exists();
        }

        void DisplayHelpTextThisFrame(string text)
        {
            Function.Call(Hash._SET_TEXT_COMPONENT_FORMAT, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._DISPLAY_HELP_TEXT_FROM_STRING_LABEL, 0, false, false, -1);
        }


        public enum PrepsPhase
        {
            Nothing,
            FadingIn,
            SpawningCars,
            FadingOut,
            Finished,
        }
        PrepsPhase PrepsStatus = PrepsPhase.Nothing;
        bool HandleRacePreps()
        {
            if (PrepsStatus == PrepsPhase.Finished) return true;
            if (PrepsStatus == PrepsPhase.Nothing)
            {
                Game.FadeScreenOut(500);
                PrepsStatus = PrepsPhase.FadingOut;
            }
            if (PrepsStatus == PrepsPhase.FadingOut && Game.IsScreenFadedOut)
            {
                PrepsStatus = PrepsPhase.SpawningCars;
                Vector3 pos = Game.Player.Character.Position;
                Function.Call(Hash.CLEAR_AREA_OF_VEHICLES, pos.X, pos.Y, pos.Z, 100, true, true, true, true, true);

            }
            if (PrepsStatus == PrepsPhase.SpawningCars)
            {
                List<int> Cars = new List<int>();

                foreach (Vehicle model in World.GetNearbyVehicles(Game.Player.Character, 30f)) Cars.Add(model.Model.Hash);
                if (Cars.Count > 3) UI.Notify("Multiple vehicles detected near the race trigger. This race will use them instead of the file.");
                Util.SpawnDrivers(Cars, AllowFilling.Checked);
                Game.FadeScreenIn(500);
                PrepsStatus = PrepsPhase.FadingIn;
                RacersFinishedTreshold = 5;
                if (!Util.IsPlayerParticipating())
                {
                    Game.Player.Character.Position = (StreetRaces.Racers[StreetRaces.Racers.Count / 2].car.Position + (StreetRaces.Racers[StreetRaces.Racers.Count / 2].car.RightVector * 5));
                    Game.Player.Character.Heading = StreetRaces.RaceHeading + 90;
                }
            }
            if (PrepsStatus == PrepsPhase.FadingIn && Game.IsScreenFadedIn)
            {
                PrepsStatus = PrepsPhase.Finished;
                return true;
            }
            return false;
        }

    }
}
