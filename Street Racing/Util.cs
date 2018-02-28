using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Drawing;
using System.Globalization;
using System.Xml;
using System.IO;
using NativeUI;

namespace Street_Races
{
    class Util
    {

        public static void WarnPlayer(string script_name, string title, string message)
        {
            Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, message);
            Function.Call(Hash._SET_NOTIFICATION_MESSAGE, "CHAR_SOCIAL_CLUB", "CHAR_SOCIAL_CLUB", true, 0, title, "~b~" + script_name);
        }

        public enum Subtask
        {
            AIMED_SHOOTING_ON_FOOT = 4,
            GETTING_UP = 16,
            MOVING_ON_FOOT_NO_COMBAT = 35,
            MOVING_ON_FOOT_COMBAT = 38,
            USING_STAIRS = 47,
            CLIMBING = 50,
            GETTING_OFF_SOMETHING = 51,
            SWAPPING_WEAPON = 56,
            REMOVING_HELMET = 92,
            DEAD = 97,
            MELEE_COMBAT = 130,
            HITTING_MELEE = 130,
            SITTING_IN_VEHICLE = 150,
            DRIVING_WANDERING = 151,
            EXITING_VEHICLE = 152,

            ENTERING_VEHICLE_GENERAL = 160,
            ENTERING_VEHICLE_BREAKING_WINDOW = 161,
            ENTERING_VEHICLE_OPENING_DOOR = 162,
            ENTERING_VEHICLE_ENTERING = 163,
            ENTERING_VEHICLE_CLOSING_DOOR = 164,

            EXIING_VEHICLE_OPENING_DOOR_EXITING = 167,
            EXITING_VEHICLE_CLOSING_DOOR = 168,
            DRIVING_GOING_TO_DESTINATION_OR_ESCORTING = 169,
            USING_MOUNTED_WEAPON = 199,
            AIMING_THROWABLE = 289,
            AIMING_GUN = 290,
            AIMING_PREVENTED_BY_OBSTACLE = 299,
            IN_COVER_GENERAL = 287,
            IN_COVER_FULLY_IN_COVER = 288,

            RELOADING = 298,

            RUNNING_TO_COVER = 300,
            IN_COVER_TRANSITION_TO_AIMING_FROM_COVER = 302,
            IN_COVER_TRANSITION_FROM_AIMING_FROM_COVER = 303,
            IN_COVER_BLIND_FIRE = 304,

            PARACHUTING = 334,
            PUTTING_OFF_PARACHUTE = 336,

            JUMPING_OR_CLIMBING_GENERAL = 420,
            JUMPING_AIR = 421,
            JUMPING_FINISHING_JUMP = 422,
        }

        public static bool IsSubttaskActive(Ped ped, Subtask task)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, (int)task);
        }
        public static bool IsDriving(Ped ped)
        {
            return (Util.IsSubttaskActive(ped, Util.Subtask.DRIVING_WANDERING) || Util.IsSubttaskActive(ped, Util.Subtask.DRIVING_GOING_TO_DESTINATION_OR_ESCORTING));
        }

        public static int GetRandomInt(int min, int max)
        {
            Random rnd = new Random();
            return rnd.Next(min, max);
        }
        public static void DisplayHelpTextThisFrame(string text)
        {
            Function.Call(Hash._SET_TEXT_COMPONENT_FORMAT, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._DISPLAY_HELP_TEXT_FROM_STRING_LABEL, 0, false, false, -1);
        }

        public static bool IsEntityAheadEntity(Entity ent1, Entity ent2, float MinAheadDistance, float MaxAheadDistance)
        {
            Vector3 pos = ent1.Position;
            bool Min = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent2, pos.X, pos.Y, pos.Z).Y > MinAheadDistance;
            bool Max = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent2, pos.X, pos.Y, pos.Z).Y > MaxAheadDistance;
            return Min && Max;
        }


        public static bool IsPosAheadEntity(Vector3 pos, Entity ent1, float MinAheadDistance, float MaxAheadDistance)
        {
            bool Min = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent1, pos.X, pos.Y, pos.Z).Y > MinAheadDistance;
            bool Max = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent1, pos.X, pos.Y, pos.Z).Y < MaxAheadDistance;
            return Min && Max;
        }

        public static bool IsPosBehindEntity(Vector3 pos, Entity ent1, float MinBehindDistance, float MaxBehindDistance)
        {
            bool Min = -Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent1, pos.X, pos.Y, pos.Z).Y > MinBehindDistance;
            bool Max = -Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent1, pos.X, pos.Y, pos.Z).Y < MaxBehindDistance;
            return Min && Max;
        }
        public static bool IsPotentiallySliding(Vehicle veh, float threshold)
        {
            return Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION_VELOCITY, veh, true).Z) > threshold;
        }
        public static bool IsSliding(Vehicle veh, float threshold)
        {
            return Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).X) > threshold;
        }

        public static bool SlidingTreshold(Vehicle v, float maxpercent)
        {
            return Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, v, true).Normalized.X)*100>=maxpercent;
        }
        public static int GetBoneIndex(Entity entity, string value)
        {
            return GTA.Native.Function.Call<int>(Hash._0xFB71170B7E76ACBA, entity, value);
        }
        public static Vector3 GetBoneCoords(Entity entity, int boneIndex)
        {
            return GTA.Native.Function.Call<Vector3>(Hash._0x44A8FCB8ED227738, entity, boneIndex);
        }
        public static List<string> Exhausts = new List<string>
    {
        "exhaust","exhaust_2","exhaust_3","exhaust_4","exhaust_5","exhaust_6","exhaust_7"
    };

        public static void ForceNitro(Vehicle veh)
        {
            if (CanWeUse(veh))
            {
                Function.Call(Hash._SET_VEHICLE_ENGINE_TORQUE_MULTIPLIER, veh, 30f);

                if (Function.Call<bool>(Hash._0x8702416E512EC454, "core"))
                {
                    float direction = veh.Heading;
                    float pitch = Function.Call<float>(Hash.GET_ENTITY_PITCH, veh);

                    foreach (string exhaust in Exhausts)
                    {
                        Vector3 offset = GetBoneCoords(veh, GetBoneIndex(veh, exhaust));
                        offset = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, veh, offset.X, offset.Y, offset.Z);

                        Function.Call(Hash._0x6C38AF3693A69A91, "core");
                        Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "veh_backfire", veh, offset.X, offset.Y, offset.Z, 0f, pitch, 0f, 1.0f, false, false, false);

                        //   Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, "scr_carsteal5_car_muzzle_flash", offset.X, offset.Y, offset.Z, 0f, pitch,, 1.0f, false, false, false);
                    }
                }
                else
                {
                    Function.Call(Hash._0xB80D8756B4668AB6, "scr_carsteal4");
                }
            }
        }


        public static void HandleAISpinoutHelp(Vehicle veh)
        {
            if (CanWeUse(veh) && veh.IsOnAllWheels)
            {
                //Util.HandleAISuddenSteering(veh); Doesn't work well yet
                if (Util.ForwardSpeed(veh) > 20f && Util.SlidingTreshold(veh, 20f))
                {
                    if (!Util.IsPotentiallySliding(veh, 1.5f))
                    {
                        if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).X > 1f && veh.SteeringAngle < 0)
                        {
                            if (StreetRaces.Debug) World.DrawMarker(MarkerType.UpsideDownCone, veh.Position + (veh.ForwardVector * -2) + veh.RightVector, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
                            Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, 0.1f, 0f, 0f, 0f, 2f, -0.2f, 0, true, true, true, true, true);
                        }
                        else
                        if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).X < -1f && veh.SteeringAngle > 0)
                        {
                            if (StreetRaces.Debug) World.DrawMarker(MarkerType.UpsideDownCone, veh.Position + (veh.ForwardVector * -2) - veh.RightVector, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue);
                            Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, -0.1f, 0f, 0f, 0f, 2f, -0.2f, 0, true, true, true, true, true);
                        }
                    }
                    /*if
                    if(Util.IsPotentiallySliding(veh,0.01f) && !Util.IsSliding(veh, 3f) && Util.ForwardSpeed(veh) > 25f)
                    {
                        World.DrawMarker(MarkerType.UpsideDownCone, veh.Position + new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 2, 2), Color.White);
                        float force = 0;

                        if ((Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).X) < 0) force = 1; else force = -1;

                        Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, force, 0f, 0f, 0f, 2f, 0f, 0, true, true, true, true, true);

                    }
                    */
                }
            }

        }
        public static bool CanWeUse(Entity entity)
        {
            return entity != null && entity.Exists();
        }

        public static bool IsPlayerParticipating()
        {
            if (StreetRaces.Racers.Count > 0 && StreetRaces.Racers[StreetRaces.Racers.Count - 1].driver.IsPlayer) return true;
            return false;
        }
        public static void HandleAICatchUp(Vehicle veh)
        {
            if (veh.IsOnAllWheels)
            {
                if (ForwardSpeed(veh) < 30f)
                {
                    //Steering cheat
                    if (!IsPotentiallySliding(veh, 2f)) Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, veh.SteeringAngle * -0.001f, 0f, 0f, 0f, 0.5f, 0f, 0, true, true, true, true, true);

                    //Acceleration cheat
                    if (Math.Abs(veh.SteeringAngle) < 30 && veh.Acceleration > 0) Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, 0f, 1f, 0f, 0f, 0f, 0f, 0, true, true, true, true, true);

                }
                if (ForwardSpeed(veh) > 15)
                {
                    //Brake cheat
                    if (!SlidingTreshold(veh, 20f) && veh.Acceleration == 0) Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, 0f, -1f, 0f, 0f, 0f, 0f, 0, true, true, true, true, true);
                }

                //Stabilize
                if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).X > 2f)
                {
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, -0.5f, 0f, 0f, 0f, 0f, 0f, 0, true, true, true, true, true);
                }
                else if (Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).X < -2f)
                {
                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, 0.5f, 0f, 0f, 0f, 0f, 0f, 0, true, true, true, true, true);
                }
            }

        }
        public static float CalculateHandicap(Vehicle veh)
        {
            // if (veh.Model.Hash == (int)VehicleHash.SlamVan) return 0.7f;

            return 0.5f;
        }
        public static bool CheckForObstaclesAhead(Vehicle v)
        {
            float spd = v.Speed;
            if (spd < 5f)  spd = 10f;

            if (spd > 100) spd = 100f;
            Vector3 direction = v.ForwardVector * (spd);// v.Position+(v.RightVector * -1) + (v.Velocity);// ChaserPed.CurrentVehicle.Velocity;
            Vector3 directionleft = (v.RightVector * -1) + (v.ForwardVector*spd);// ChaserPed.CurrentVehicle.Velocity;
            Vector3 directionright = (v.RightVector * 1) + (v.ForwardVector * spd);// ChaserPed.CurrentVehicle.Velocity;
                                                                            //Vector3 direction = ChaserPed.CurrentVehicle.Position + ChaserPed.CurrentVehicle.ForwardVector * (ChaserPed.CurrentVehicle.Speed*2f);// ChaserPed.CurrentVehicle.Velocity;
            Vector3 origin = v.Position + (v.ForwardVector * (v.Model.GetDimensions().Y / 2));

            RaycastResult cast = World.Raycast(origin, direction, 2f, IntersectOptions.Map | IntersectOptions.Objects, v);
            RaycastResult castleft = World.Raycast(origin, directionleft, 2f, IntersectOptions.Map | IntersectOptions.Objects, v);
            RaycastResult castright = World.Raycast(origin, directionright, 2f, IntersectOptions.Map | IntersectOptions.Objects, v);

            UI.ShowSubtitle(cast.SurfaceNormal.Z.ToString(), 1000);
            //DrawLine(origin, cast.HitCoords);
            //DrawLine(origin, castleft.HitCoords);
            //DrawLine(origin, castright.HitCoords);
            if ((cast.DitHitAnything && Math.Abs(cast.SurfaceNormal.Z) < 0.8)
                || (castleft.DitHitAnything && Math.Abs(castleft.SurfaceNormal.Z) < 0.8)
                || (castright.DitHitAnything && Math.Abs(castright.SurfaceNormal.Z) < 0.8))
            {

               // UI.Notify(v+ " WALL");
                return true;
            }

            return false;
        }
        public static bool CanAccessAreaFromHere(Vehicle veh, Vector3 pos, int height_offset)
        {
            if (veh.Position.DistanceTo(pos) < 300f)
            {
                RaycastResult raycast = World.Raycast(veh.Position + new Vector3(0, 0, height_offset), pos + new Vector3(0, 0, height_offset), IntersectOptions.Map);                
                if (!raycast.DitHitAnything || Vector3.Angle(raycast.SurfaceNormal,Vector3.WorldUp)<45) return true;
            }
            return false;
        }
        public static bool CarCanSeePos(Vehicle veh, Vector3 pos, int height_offset)
        {
            //if (veh.Position.DistanceTo(pos) < 50f) return true;
            if (veh.Position.DistanceTo(pos) < 100f)
            {
                RaycastResult raycast = World.Raycast(veh.Position + new Vector3(0, 0, height_offset), pos + new Vector3(0, 0, height_offset), IntersectOptions.Map);
                
                if (!raycast.DitHitAnything) return true;
            }
            return false;
        }

        public static float GetPitch(Vehicle veh)
        {
            return Function.Call<float>(Hash.GET_ENTITY_PITCH, veh);
        }

        public static bool HandleAIOutOfPath(Vehicle veh, bool avoidjumps, bool offroadmode, bool flatmode)
        {
            if (CanWeUse(veh) && ((veh.IsOnAllWheels && ForwardSpeed(veh) > 20f) || offroadmode))
            {
                if (flatmode && veh.SteeringAngle > 10) return true;

                if (veh.SteeringAngle > 30)
                {
                    if (StreetRaces.Debug) UI.Notify(veh.FriendlyName + " brake: steerangle too high");
                    return true;
                }
                float multiplier = 1.0f;
                if (veh.Rotation.X < -4) multiplier = 2f;
                Vector3 RealDirection = veh.Position + (veh.UpVector * -veh.HeightAboveGround) + (veh.Velocity * CalculateHandicap(veh) * multiplier);
                Vector3 SteeredDirection = (RealDirection + (veh.RightVector * -(veh.SteeringAngle / 20)) * multiplier);
                Vector3 SteeredDirectionToGround = SteeredDirection;

                RaycastResult toground = World.Raycast(SteeredDirection + new Vector3(0, 0, 3), SteeredDirection + new Vector3(0, 0, -3), IntersectOptions.Map);

                if (toground.HitCoords != Vector3.Zero) SteeredDirectionToGround = toground.HitCoords;

                if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, SteeredDirectionToGround, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), Color.Yellow);
                if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, SteeredDirection, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), Color.Green);


                //DisplayHelpTextThisFrame(raycast.HitCoords.DistanceTo(direction).ToString());

                //Jumps
                if (offroadmode && Util.ForwardSpeed(veh) > 10f && (SteeredDirection.Z > toground.HitCoords.Z + 1f))
                {
                    if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, SteeredDirection, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), Color.Red);
                    return true;
                }
                if (avoidjumps && Math.Abs(veh.SteeringAngle) > 20 && (SteeredDirection.Z - toground.HitCoords.Z > 0.7f))
                {
                    if (StreetRaces.Debug) World.DrawMarker(MarkerType.UpsideDownCone, SteeredDirection + new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(2, 2, 2), Color.Black);
                    {
                        if (StreetRaces.Debug) UI.Notify(veh.FriendlyName + " brake: jump detected");
                        return true;
                    }
                }

                //Out of path
                if (!SlidingTreshold(veh, 20f) && IsPointOnRoad(veh.Position, veh) && !IsPointOnRoad(SteeredDirectionToGround, veh)) // || raycastoutofpath.HitCoords.DistanceTo(posoutofroad)>4f)
                {
                    if (World.GetNextPositionOnStreet(SteeredDirectionToGround).DistanceTo(SteeredDirectionToGround) > 20f || (SteeredDirection.Z - toground.HitCoords.Z > 2f))
                    {
                        if (StreetRaces.Debug) World.DrawMarker(MarkerType.UpsideDownCone, World.GetNextPositionOnStreet(SteeredDirectionToGround) + new Vector3(0, 0, 1.5f) + new Vector3(0, 0, -0.4f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red);
                        {
                            if (StreetRaces.Debug) UI.Notify(veh.FriendlyName + " brake: potential out of path");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool HandleAISlamToWall(Vehicle veh)
        {
            if (veh.IsOnAllWheels && ForwardSpeed(veh) > 10f) // && Math.Abs(veh.SteeringAngle) > 10f
            {
                Vector3 direction = veh.Position + veh.Velocity;
                Vector3 origin = veh.Position + (veh.ForwardVector * (veh.Model.GetDimensions().Y));

                //Walls
                RaycastResult raycastoutofpath = World.Raycast(origin, direction, IntersectOptions.Map);

                if (StreetRaces.Debug) DrawLine(origin, direction, Color.Red);

                Vector3 marker = direction;
                if (raycastoutofpath.DitHitAnything)
                {
                    marker = raycastoutofpath.HitCoords;

                    //Walls

                    //World.DrawMarker(MarkerType.ThinChevronUp, raycast.HitCoords, raycast.SurfaceNormal, new Vector3(0, 0, 0), new Vector3(2, 2, 2), Color.Red);

                    if (StreetRaces.Debug) World.DrawMarker(MarkerType.ThinChevronUp, marker, raycastoutofpath.SurfaceNormal, new Vector3(0, 0, 0), new Vector3(3f, 3f, 3f), Color.Red);

                    if (raycastoutofpath.HitCoords != Vector3.Zero)
                    {
                        if (Math.Abs(raycastoutofpath.SurfaceNormal.Z) < 0.8f)
                        {
                            if (StreetRaces.Debug) UI.Notify(veh.FriendlyName + " brake: wall detected");
                            return true;
                        }
                        //Vertical obstacles
                        //if (raycastoutofpath.HitCoords.DistanceTo(direction) > 2f)  return true; //Jumps
                    }

                }
            }
            return false;
        }

        public static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, color.R, color.G, color.B, color.A);
        }
        public static void ForceBrake(Vehicle veh) //, float force
        {
            if (CanWeUse(veh) && veh.IsOnAllWheels)
            {
                Ped driver = veh.GetPedOnSeat(VehicleSeat.Driver);
                if (CanWeUse(driver)) driver.DrivingSpeed = veh.Speed * 0.8f;
            }
        }

        public static void ForceHandBrake(Vehicle veh) //, float force
        {
            if (CanWeUse(veh) && veh.IsOnAllWheels)
            {

                if (CanWeUse(veh)) veh.HandbrakeOn = true;
                float multiplier = veh.SteeringAngle;
                if (multiplier > 3) multiplier = 3;
                // temp Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, veh.SteeringAngle * -0.005f, 0f, 0f, 0f, 0.5f, 0f, 0, true, true, true, true, true);
                //Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, 0.1f, 0f, 0f, 0f, 0.1f * -multiplier, -0.1f, 0, true, true, true, true, true);

            }
        }
        public static void HandleAISuddenSteering(Vehicle veh)
        {
            if (veh.IsOnAllWheels && Math.Abs(veh.SteeringAngle) > 30 && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y > 40f) // IsPotentiallySliding(veh, 1.5f) && !IsSliding(veh, 0.5f) &&
            {
                if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, veh.Position + new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Orange);
                Function.Call(Hash.APPLY_FORCE_TO_ENTITY, veh, 3, Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION_VELOCITY, veh, true).Z * -0.1f, 0f, 0f, 0f, -5f, 0f, 0, true, true, true, true, true);
            }
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static Racer GetRacerInFirst()
        {
            foreach (Racer racer in StreetRaces.Racers) if (racer.PositionInRace == 1) return racer;
            return null;
        }
        public static Racer GetPlayerRacer()
        {
            foreach (Racer racer in StreetRaces.Racers) if (racer.driver == Game.Player.Character) return racer;
            return null;
        }
        public static float HeadingDifference(Entity A, Entity b)
        {
            return Math.Abs(Function.Call<float>(Hash._GET_ENTITY_PHYSICS_HEADING, A) - Function.Call<float>(Hash._GET_ENTITY_PHYSICS_HEADING, A));
        }

        public static bool IsStable(Vehicle v, float maxSlide, float maxSpin)
        {
            if (Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION_VELOCITY, v, true).Z) > maxSpin) return false;
            if (Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, v, true).Normalized.X) * 100 >= maxSlide) return false;
            return true;
        }
        public static float HandleAIRearEndHelpSP(Racer r)
        {

            Vehicle veh = r.car;

            float height = veh.HeightAboveGround;
            if (height < 0.4f || height>2f) height = 1f;
            if (CanWeUse(veh) &&  veh.IsOnAllWheels && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y > 2f)
            {

                float CurrentSpeed = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y;
                Vector3 direction = (veh.Velocity*1.5f);
                RaycastResult ray = World.RaycastCapsule(veh.Position+new Vector3(0,0,height) + (veh.ForwardVector * (veh.Model.GetDimensions().Y)), direction, 1f, (veh.Model.GetDimensions().X /2.5f), IntersectOptions.Everything, veh); //(veh.ForwardVector*5
                Entity ent = ray.HitEntity;
                Vector3 hitpos = ray.HitCoords;
                Vector3 refpos = veh.Position + (veh.ForwardVector * (veh.Model.GetDimensions().Y / 2));
               if(StreetRaces.Debug && Game.Player.Character.IsSittingInVehicle(veh)) DrawLine(refpos, hitpos, Color.Green);
                //World.DrawMarker(MarkerType.DebugSphere, hitpos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 1f), Color.Yellow);

                if (CanWeUse(ent) && ent.Model.IsCar && ent.Velocity.Length()<veh.Velocity.Length())
                {

                    bool IsRacer = false;
                    if (ent.CurrentBlip.Exists() && ent.CurrentBlip.Color == BlipColor.Blue) IsRacer = true;

                    //Against traffic
                    if (!IsRacer && ent.Velocity.Length()>15f && Math.Abs(Function.Call<float>(Hash._GET_ENTITY_PHYSICS_HEADING, veh) - Function.Call<float>(Hash._GET_ENTITY_PHYSICS_HEADING, ent))>50f)
                    {
                        r.LetOffTheGas = false;
                        if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, hitpos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 1f), Color.Black);
                        return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y *0.7f;
                    }

                    //Collission
                    if (veh.IsTouching(ent))
                    {
                        r.LetOffTheGas = true;
                        r.HoldBrakeTime = Game.GameTime + 1000;
                            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y - 3f;
                    }

                    if (refpos.DistanceTo(hitpos) < 3f && !IsRacer)
                    {
                        if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, hitpos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 1f), Color.Red);
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 1f)
                        {
                            if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 10f)
                            {
                                r.LetOffTheGas = false;
                            }
                            else
                            {
                                r.LetOffTheGas = true;
                            }
                            r.HoldBrakeTime = Game.GameTime + 1000;
                            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y  -2f;
                        }
                    }
                    if (refpos.DistanceTo(hitpos) < 15f)
                    {
                        if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, hitpos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 1f), Color.Orange);
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 15f)
                        {
                            r.LetOffTheGas = false;
                            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y + 10f;

                        }
                        if (!IsRacer &&  Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 6f && HeadingDifference(veh, ent) < 10)
                        {
                            r.LetOffTheGas = true;
                            r.HoldBrakeTime = Game.GameTime + 500;
                            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y;
                        }


                        //if ((veh.Velocity.Length()) > ent.Velocity.Length()) return 800;
                    }
                    if (!IsRacer && refpos.DistanceTo(hitpos) < 20f && HeadingDifference(veh, ent) < 5)
                    {
                        if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, hitpos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 1f), Color.Yellow);                       
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 30f) return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y;
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 20f) return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y + 15f;
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 15f)
                        {
                            r.LetOffTheGas = true;
                            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y + 14f;
                        }
                    }
                    if (!IsRacer && refpos.DistanceTo(hitpos) < 40f && IsRoadBusy(refpos, 5))
                    {
                        if (StreetRaces.Debug) World.DrawMarker(MarkerType.DebugSphere, hitpos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 1f, 1f), Color.Green);
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 40f) return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y - 20f;
                        if (Math.Abs(veh.Velocity.Length() - ent.Velocity.Length()) > 25f)
                        {
                            r.LetOffTheGas = true;
                            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y - 5f;
                        }
                       // return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ent, true).Y + 10f;
                    }
                }
            }
            return 0;
        }

        public static int CountDownValue = 3;
        public static int CountdownRef = Game.GameTime;
        public static bool Countdown()
        {
            if (CountdownRef < Game.GameTime)
            {
                string Countdown = CountDownValue.ToString();
                if (CountDownValue == 0) Countdown = "GO";
                BigMessageThread.MessageInstance.ShowColoredShard(Countdown, "", HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_MENU_YELLOW, 800);
                CountdownRef = Game.GameTime + 1500;

                if (CountDownValue > 0)
                {
                    CountDownValue--;
                    return false;
                }
                else CountDownValue = 3; return true;

            }
            return false;
        }

        public static float ForwardSpeed(Vehicle veh)
        {
            if (CanWeUse(veh)) return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, veh, true).Y; else return 0;
        }

        public static bool IsRaining()
        {
            int weather = Function.Call<int>(GTA.Native.Hash._0x564B884A05EC45A3); //get current weather hash
            switch (weather)
            {
                case (int)Weather.Blizzard:
                    {
                        return true;
                    }
                case (int)Weather.Clearing:
                    {
                        return true;
                    }
                case (int)Weather.Foggy:
                    {
                        return true;
                    }
                case (int)Weather.Raining:
                    {
                        return true;
                    }
                case (int)Weather.Neutral:
                    {
                        return true;
                    }
                case (int)Weather.ThunderStorm:
                    {
                        return true;
                    }
                case (int)Weather.Snowlight:
                    {
                        return true;
                    }
                case (int)Weather.Snowing:
                    {
                        return true;
                    }
                case (int)Weather.Christmas:
                    {
                        return true;
                    }
            }
            return false;
        }
        public static bool IsNightTime()
        {
            int hour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
            return (hour > 20 || hour < 7);
        }

        public enum Nodetype { AnyRoad, Road, Offroad, Water }
        public static Vector3 GenerateSpawnPos(Vector3 desiredPos, Nodetype roadtype, bool sidewalk)
        {

            Vector3 finalpos = Vector3.Zero;
            bool ForceOffroad = false;


            OutputArgument outArgA = new OutputArgument();
            int NodeNumber = 1;
            int type = 0;

            if (roadtype == Nodetype.AnyRoad) type = 1;
            if (roadtype == Nodetype.Road) type = 0;
            if (roadtype == Nodetype.Offroad) { type = 1; ForceOffroad = true; }
            if (roadtype == Nodetype.Water) type = 3;


            int NodeID = Function.Call<int>(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_ID, desiredPos.X, desiredPos.Y, desiredPos.Z, NodeNumber, type, 300f, 300f);
            if (ForceOffroad)
            {
                while (!Function.Call<bool>(Hash._GET_IS_SLOW_ROAD_FLAG, NodeID) && NodeNumber < 500)
                {
                    NodeNumber++;
                    NodeID = Function.Call<int>(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_ID, desiredPos.X, desiredPos.Y, desiredPos.Z, NodeNumber + 5, type, 300f, 300f);
                }
            }
            Function.Call(Hash.GET_VEHICLE_NODE_POSITION, NodeID, outArgA);
            finalpos = outArgA.GetResult<Vector3>();

            if (sidewalk) finalpos = World.GetNextPositionOnSidewalk(finalpos);
            return finalpos;
        }


     public  enum  PathnodeFlags
        {
            Slow=1,
            Two=2,
            Intersection=4,
            Eight=8,SlowTraffic=12, ThirtyTwo=32, Freeway=64, FourWayIntersection=128, BigIntersectionLeft=512
        }
        public static void GetRoadFlags(Vector3 pos)
        {
            OutputArgument outArgA = new OutputArgument();
            OutputArgument outArgB = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_VEHICLE_NODE_PROPERTIES, pos.X, pos.Y, pos.Z, outArgA, outArgB))
            {
                int busy = outArgA.GetResult<int>();
                int flags = outArgB.GetResult<int>();

                string d = "";
                foreach (int flag in Enum.GetValues(typeof(PathnodeFlags)).Cast<PathnodeFlags>())
                {

                    if ((flag & flags) != 0) d += " " + (PathnodeFlags)flag;
                }
                DisplayHelpTextThisFrame("Flags: "+d);
            }
        }
        public static bool RoadHasFlag(Vector3 pos, PathnodeFlags flag)
        {
            OutputArgument outArgA = new OutputArgument();
            OutputArgument outArgB = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_VEHICLE_NODE_PROPERTIES, pos.X, pos.Y, pos.Z, outArgA, outArgB))
            {
                int busy = outArgA.GetResult<int>();
                int flags = outArgB.GetResult<int>();               
                if ((flags & (int)flag) != 0) return true;
            }
            return false;
        }
        bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        public static bool IsRoadBusy(Vector3 pos, int carNum)
        {
            OutputArgument outArgA = new OutputArgument();
            OutputArgument outArgB = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_VEHICLE_NODE_PROPERTIES, pos.X, pos.Y, pos.Z, outArgA, outArgB))
            {
                int busy = outArgA.GetResult<int>();
                int flags = outArgB.GetResult<int>();

                //DisplayHelpTextThisFrame("Busy:" + busy + "~n~Flags:" + flags);
                if (busy >= carNum) return true;

                //BOOL GET_VEHICLE_NODE_PROPERTIES(float x, float y, float z, int *density, int* flags) // 0x0568566ACBB5DEDC 0xCC90110B
            }
            return false;
        }
        public static bool IsPointOnRoad(Vector3 pos, Entity ent)
        {
            return Function.Call<bool>(Hash.IS_POINT_ON_ROAD, pos.X, pos.Y, pos.Z, ent);
        }
        public static bool IsPointNeaRoad(Vector3 pos)
        {
            return pos.DistanceTo(World.GetNextPositionOnStreet(pos, true)) < 10f;
        }
        public static bool IsNumeric(string text)
        {
            int n;
            return int.TryParse(text, out n);
        }

        public static void GetClasses()
        {
            StreetRaces.RaceClasses.Clear();
            XmlDocument document = new XmlDocument();
            document.Load(@"scripts\\" + StreetRaces.ScriptFolder + "/Racers/Racers.xml");

            int patienced = 0;
            while (document == null)
            {
                patienced++;
                Script.Wait(0);
                if (patienced > 50)
                {
                    UI.Notify("Coudn't load Racers file.");
                    return;
                }
            }

            XmlElement docroot = document.DocumentElement;
            XmlNodeList nodelist = docroot.SelectNodes("//Racers/*");

            foreach (XmlElement driver in nodelist)
            {
                if (!StreetRaces.RaceClasses.Contains(driver.GetAttribute("Class")))
                {
                    StreetRaces.RaceClasses.Add(driver.GetAttribute("Class"));
                }
            }
        }

        public static void SpawnDrivers(List<int> AddedCars, bool allow_filler)
        {

            bool OnlyAddedCars = AddedCars.Count > 3;

            List<String> drivers = new List<String>();
            List<String> pickeddrivers = new List<String>();

            List<string> CarsForFilling = new List<string>();
            int Filler = 0;
            StreetRaces.MaxRacers = StreetRaces.SetupCarsNumber.IndexToItem(StreetRaces.SetupCarsNumber.Index);

            Vector3 position = StreetRaces.RaceWaypoints[0];
            int number = -1;
            float back = 0;
            if (OnlyAddedCars)
            {

                for (int _ = 0; _ < StreetRaces.MaxRacers; _++)
                {
                    number++;

                    //if (StreetRaces.Debug) UI.Notify("Filling... " + AddedCars[0].ToString());
                   if(StreetRaces.Racers.Count > 0)
                    {

                        if (IsOdd(number)) position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back));
                        else position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)) + (StreetRaces.Racers[0].car.RightVector * 2);
                    }

                    int vehmodel = AddedCars[GetRandomInt(0, AddedCars.Count - 1)];
                    Vehicle veh = null;
                    veh = World.CreateVehicle(vehmodel, position, StreetRaces.RaceHeading);
                    Ped ped = World.CreatePed(StreetRaces.DriverModels[Util.GetRandomInt(0, StreetRaces.DriverModels.Count)], veh.Position.Around(2f));
                    ped.SetIntoVehicle(veh, VehicleSeat.Driver);


                    //Tuning
                    if (StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.All || StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.NonTuned) RandomTuning(veh);
                    back += veh.Model.GetDimensions().Y + 0.5f;

                    veh.IsCollisionProof = true;
                    veh.IsAxlesStrong = true;
                    StreetRaces.Racers.Add(new Racer(veh, ped, veh.FriendlyName));
                    Script.Wait(250);
                }
            }
        
            else
            {

                XmlDocument document = new XmlDocument();
                document.Load(@"scripts\\" + StreetRaces.ScriptFolder + "/Racers/Racers.xml");

                int patienced = 0;
                while (document == null)
                {
                    patienced++;
                    Script.Wait(0);
                    if (patienced > 50)
                    {
                        UI.Notify("Coudn't load Racers file.");
                        return;
                    }
                }

                XmlElement docroot = document.DocumentElement;
                XmlNodeList nodelist = docroot.SelectNodes("//Racers/*");
                //XmlNodeList nodelist2 = nodelist;


                foreach (XmlElement driver in nodelist)
                {
                    if (driver.GetAttribute("Class") == StreetRaces.ClassList[StreetRaces.CarClass.Index].ToString())
                    {
                        if (1 == 1)
                        {
                            drivers.Add(driver.GetAttribute("Name"));
                        }
                    }
                }


                if (StreetRaces.MaxRacers > drivers.Count)
                {

                    if (allow_filler)
                    {
                        Util.AddNotification("CHAR_SOCIAL_CLUB", "~b~" + StreetRaces.ScriptName, "NOT ENOUGH RACERS", "There's not enough racers installed. Filling with more cars from the list...");
                        Filler = StreetRaces.MaxRacers - drivers.Count;
                        StreetRaces.MaxRacers = drivers.Count;
                    }
                    else
                    {
                        StreetRaces.MaxRacers = drivers.Count;

                    }
                }
                //UI.Notify("Not enough cars for this Race, lowering limit."); 

                var copyDrivers = new List<string>(drivers);
                for (int _ = 0; _ < StreetRaces.MaxRacers; _++)
                {
                    int i = GetRandomInt(0, copyDrivers.Count);
                    pickeddrivers.Add(copyDrivers[i]);
                    copyDrivers.RemoveAt(i);
                }
                //UI.Notify(pickeddrivers.Count + " Drivers found.");


                foreach (XmlElement driver in nodelist)
                {
                    if (pickeddrivers.Contains(driver.GetAttribute("Name")) && driver.GetAttribute("Class") == StreetRaces.ClassList[StreetRaces.CarClass.Index].ToString())
                    {
                        number++;
                        //UI.Notify("Spawning " + driver.GetAttribute("Name"));
                        //File.WriteAllText(@"scripts\\DragMeets\debug.txt", driver.GetAttribute("DriverName"));

                        if (StreetRaces.Racers.Count > 0)
                        {
                            if (!IsOdd(number)) position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)); else position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)) + (StreetRaces.Racers[0].car.RightVector * 2);
                        }

                        string model = driver.SelectSingleNode("Vehicle/Model").InnerText;
                        Vehicle veh;
                        if (IsNumeric(model))
                        {
                            veh = World.CreateVehicle(int.Parse(model), position, StreetRaces.RaceHeading);

                        }
                        else
                        {
                            veh = World.CreateVehicle(model, position, StreetRaces.RaceHeading);

                        }
                        CarsForFilling.Add(model);

                        int patience = 0;
                        while (!CanWeUse(veh))
                        {
                            patience++;
                            DisplayHelpTextThisFrame("Trying to spawn " + driver.SelectSingleNode("Vehicle/Model").InnerText + " for the Race...(" + patience + "/100)");
                            if (patience > 99)
                            {
                                WarnPlayer(StreetRaces.ScriptName + " " + StreetRaces.ScriptVer, "VEHICLE LOAD ERROR", "Error trying to load " + driver.SelectSingleNode("Vehicle/Model").InnerText + ". Aborting Race.");
                                StreetRaces.ShouldForceClean = true;
                                break;
                            }
                            Script.Wait(0);
                        }
                        //File.AppendAllText(@"scripts\\DragMeets\debug.txt", " - " + " created");

                        Ped ped;
                        if (driver.SelectSingleNode("PedModel") == null || driver.SelectSingleNode("PedModel").InnerText == "random")
                        {
                            ped = World.CreatePed(StreetRaces.DriverModels[Util.GetRandomInt(0, StreetRaces.DriverModels.Count)], veh.Position.Around(2f));
                        }
                        else
                        {
                            ped = World.CreatePed(driver.SelectSingleNode("PedModel").InnerText, veh.Position.Around(2f));
                        }
                        while (veh == null || ped == null)
                        {
                            Script.Wait(0);
                        }
                        //File.AppendAllText(@"scripts\\DragMeets\debug.txt", " - " + " ped created");


                        //UI.Notify("Tuning " + driver.GetAttribute("Name"));
                        int PartsTuned = 0;
                        Function.Call(Hash.SET_VEHICLE_MOD_KIT, veh, 0);

                        if (StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.NonTuned)
                        {

                            if (driver.SelectSingleNode("Vehicle/PrimaryColor") != null) veh.PrimaryColor = (VehicleColor)int.Parse(driver.SelectSingleNode("Vehicle/PrimaryColor").InnerText, CultureInfo.InvariantCulture);
                            else
                            {
                                var color = Enum.GetValues(typeof(VehicleColor));

                                Random random = new Random();
                                veh.PrimaryColor = (VehicleColor)color.GetValue(random.Next(color.Length));

                                Random random2 = new Random();
                                veh.SecondaryColor = (VehicleColor)color.GetValue(random2.Next(color.Length));

                                if (veh.LiveryCount > 0) veh.Livery = GetRandomInt(0, veh.LiveryCount);
                            }



                            if (driver.SelectSingleNode("Vehicle/SecondaryColor") != null) veh.SecondaryColor = (VehicleColor)int.Parse(driver.SelectSingleNode("Vehicle/SecondaryColor").InnerText, CultureInfo.InvariantCulture);
                            if (driver.SelectSingleNode("Vehicle/PearlescentColor") != null) veh.PearlescentColor = (VehicleColor)int.Parse(driver.SelectSingleNode("Vehicle/PearlescentColor").InnerText, CultureInfo.InvariantCulture);
                            if (driver.SelectSingleNode("Vehicle/TrimColor") != null) Function.Call((Hash)0xF40DD601A65F7F19, veh, int.Parse(driver.SelectSingleNode("Vehicle/TrimColor").InnerText, CultureInfo.InvariantCulture));
                            if (driver.SelectSingleNode("Vehicle/DashColor") != null) Function.Call((Hash)0x6089CDF6A57F326C, veh, int.Parse(driver.SelectSingleNode("Vehicle/DashColor").InnerText, CultureInfo.InvariantCulture));
                            if (driver.SelectSingleNode("Vehicle/RimColor") != null) veh.RimColor = (VehicleColor)int.Parse(driver.SelectSingleNode("Vehicle/RimColor").InnerText, CultureInfo.InvariantCulture);

                            if (driver.SelectSingleNode("Vehicle/LicensePlateText") != null) veh.NumberPlate = driver.SelectSingleNode("Vehicle/LicensePlateText").InnerText;
                            if (driver.SelectSingleNode("Vehicle/LicensePlate") != null) Function.Call(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, veh, int.Parse(driver.SelectSingleNode("Vehicle/LicensePlate").InnerText, CultureInfo.InvariantCulture));
                            if (driver.SelectSingleNode("Vehicle/WindowsTint") != null) veh.WindowTint = (VehicleWindowTint)int.Parse(driver.SelectSingleNode("Vehicle/WindowsTint").InnerText, CultureInfo.InvariantCulture);


                            if (driver.SelectSingleNode("Vehicle/WheelType") != null) veh.WheelType = (VehicleWheelType)int.Parse(driver.SelectSingleNode("Vehicle/WheelType").InnerText, CultureInfo.InvariantCulture);

                            if (driver.SelectSingleNode("Vehicle/SmokeColor") != null)
                            {
                                Color color = Color.FromArgb(255, int.Parse(driver.SelectSingleNode("Vehicle/SmokeColor/Color/R").InnerText), int.Parse(driver.SelectSingleNode("Vehicle/SmokeColor/Color/G").InnerText), int.Parse(driver.SelectSingleNode("Vehicle/SmokeColor/Color/B").InnerText));
                                veh.TireSmokeColor = color;
                            }

                            if (driver.SelectSingleNode("Vehicle/NeonColor") != null)
                            {
                                Color color = Color.FromArgb(255, int.Parse(driver.SelectSingleNode("Vehicle/NeonColor/R").InnerText), int.Parse(driver.SelectSingleNode("Vehicle/NeonColor/G").InnerText), int.Parse(driver.SelectSingleNode("Vehicle/NeonColor/B").InnerText));
                                veh.NeonLightsColor = color;
                            }

                            if (driver.SelectSingleNode("Vehicle/Neons/Back") != null) veh.SetNeonLightsOn(VehicleNeonLight.Back, bool.Parse(driver.SelectSingleNode("Vehicle/Neons/Back").InnerText));
                            if (driver.SelectSingleNode("Vehicle/Neons/Front") != null) veh.SetNeonLightsOn(VehicleNeonLight.Front, bool.Parse(driver.SelectSingleNode("Vehicle/Neons/Front").InnerText));
                            if (driver.SelectSingleNode("Vehicle/Neons/Left") != null) veh.SetNeonLightsOn(VehicleNeonLight.Left, bool.Parse(driver.SelectSingleNode("Vehicle/Neons/Left").InnerText));
                            if (driver.SelectSingleNode("Vehicle/Neons/Right") != null) veh.SetNeonLightsOn(VehicleNeonLight.Right, bool.Parse(driver.SelectSingleNode("Vehicle/Neons/Right").InnerText));

                            if (driver.SelectSingleNode("Vehicle/Components") != null)
                            {
                                foreach (XmlElement component in driver.SelectNodes("Vehicle/Components/*"))
                                {
                                    int extra = 0;
                                    if (int.Parse(component.InnerText, CultureInfo.InvariantCulture) == 0) extra = -1;
                                    Function.Call(Hash.SET_VEHICLE_EXTRA, veh, int.Parse(component.GetAttribute("ComponentIndex")), extra);
                                }
                            }
                            if (driver.SelectSingleNode("Vehicle/ModToggles") != null)
                            {
                                foreach (XmlElement component in driver.SelectNodes("Vehicle/ModToggles/*"))
                                {
                                    Function.Call(Hash.TOGGLE_VEHICLE_MOD, veh, int.Parse(component.GetAttribute("ToggleIndex")), bool.Parse(component.InnerText));
                                }

                            }

                            if (driver.SelectSingleNode("Vehicle/Mods") != null)
                            {
                                if (driver.SelectSingleNode("Vehicle/CustomTires") != null)
                                {
                                    foreach (XmlElement component in driver.SelectNodes("Vehicle/Mods/*"))
                                    {
                                        PartsTuned++;
                                        veh.SetMod((VehicleMod)int.Parse(component.GetAttribute("ModIndex")), int.Parse(component.InnerText, CultureInfo.InvariantCulture), bool.Parse(driver.SelectSingleNode("Vehicle/CustomTires").InnerText));
                                    }
                                }
                                else
                                {
                                    foreach (XmlElement component in driver.SelectNodes("Vehicle/Mods/*"))
                                    {
                                        PartsTuned++;
                                        veh.SetMod((VehicleMod)int.Parse(component.GetAttribute("ModIndex")), int.Parse(component.InnerText, CultureInfo.InvariantCulture), false);
                                    }
                                }
                            }

                            if (PartsTuned == 0 && StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.NonTuned)
                            {
                                if (StreetRaces.Debug) UI.Notify("Autotuning " + veh.FriendlyName);
                                RandomTuning(veh);

                            }

                            if (driver.SelectSingleNode("CustomTurbo") != null)
                            {
                                veh.EnginePowerMultiplier = int.Parse(driver.SelectSingleNode("CustomTurbo").InnerText, CultureInfo.InvariantCulture);
                            }

                        }
                        else
                        {
                            if (StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.All)
                            {
                                RandomTuning(veh);
                            }
                        }


                        ped.SetIntoVehicle(veh, VehicleSeat.Driver);


                        ped.CanFlyThroughWindscreen = false;
                        Function.Call(Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, ped, false);
                        Function.Call(Hash.SET_PED_PATH_CAN_DROP_FROM_HEIGHT, ped, true);
                        Function.Call(Hash.SET_PED_PATH_AVOID_FIRE, ped, true);
                        Function.Call(Hash.SET_PED_PATHS_WIDTH_PLANT, ped, 10.0f);

                        back += veh.Model.GetDimensions().Y + 0.5f;
                        Script.Wait(500);
                        string name = driver.GetAttribute("Name");
                        if (StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.All) name = veh.FriendlyName;
                        StreetRaces.Racers.Add(new Racer(veh, ped, name));

                    }
                }

                if (CarsForFilling.Count > 0)
                {
                    for (int _ = 0; _ < Filler; _++)
                    {
                        number++;

                        if (StreetRaces.Debug) UI.Notify("Filling... " + CarsForFilling[0].ToString());
                        if (IsOdd(number)) position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)); else position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)) + (StreetRaces.Racers[0].car.RightVector * 2);

                        string vehmodel = CarsForFilling[GetRandomInt(0, CarsForFilling.Count - 1)];
                        Vehicle veh = null;
                        if (IsNumeric(vehmodel)) veh = World.CreateVehicle(int.Parse(vehmodel), position, StreetRaces.RaceHeading); else veh = World.CreateVehicle(vehmodel, position, StreetRaces.RaceHeading);
                        Ped ped = World.CreatePed(StreetRaces.DriverModels[Util.GetRandomInt(0, StreetRaces.DriverModels.Count)], veh.Position.Around(2f));
                        ped.SetIntoVehicle(veh, VehicleSeat.Driver);


                        //Tuning
                        if (StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.All || StreetRaces.ForceTuneconfig.Index == (int)Tuneconfig.NonTuned) RandomTuning(veh);

                        back += StreetRaces.Racers[0].car.Model.GetDimensions().Y + 0.5f;

                        StreetRaces.Racers.Add(new Racer(veh, ped, veh.FriendlyName));
                        Script.Wait(250);
                    }
                }
            }
            if (CanWeUse(Game.Player.Character.CurrentVehicle))
            {

                //if (!IsOdd(number)) position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * - (back)); else position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * - (back)) + (StreetRaces.Racers[0].car.RightVector * 2);
                //back += StreetRaces.Racers[0].car.Model.GetDimensions().Y + 0.5f;

                if (IsOdd(number)) position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)); else position = StreetRaces.Racers[0].car.Position + (StreetRaces.Racers[0].car.ForwardVector * -(back)) + (StreetRaces.Racers[0].car.RightVector * 2);

                Game.Player.Character.CurrentVehicle.Position = position;
                Game.Player.Character.CurrentVehicle.Heading = StreetRaces.RaceHeading;

                if (StreetRaces.AIGodmode.Checked)
                {
                    Game.Player.Character.CurrentVehicle.IsInvincible = true;
                    Game.Player.Character.CurrentVehicle.IsAxlesStrong = true;
                }
                StreetRaces.Racers.Add(new Racer(Game.Player.Character.CurrentVehicle, Game.Player.Character, Game.Player.Name));
            }
            else
            {
                //if(CanWeUse(StreetRaces.Heli)) Game.Player.Character.SetIntoVehicle(StreetRaces.Heli, VehicleSeat.Passenger);
                // if (CanWeUse(StreetRaces.Heli)) Game.Player.Character.SetIntoVehicle(Util.GetRacerInFirst().car, VehicleSeat.Passenger);
            }

            StreetRaces.MaxRacers = StreetRaces.Racers.Count;
        }
        public static void FlareUpNextCheckpoint(Vector3 pos)
        {
            int flare = Game.GenerateHash("WEAPON_FLAREGUN");

            if (!Function.Call<bool>(Hash.HAS_WEAPON_ASSET_LOADED, flare))
            {
                Function.Call(Hash.REQUEST_WEAPON_ASSET, flare);
                //while (!Function.Call<bool>(Hash.HAS_WEAPON_ASSET_LOADED, flare)) Script.Wait(0);
            }
            else
            {
                if (!Function.Call<bool>(Hash.IS_PROJECTILE_IN_AREA, pos.X - 5, pos.Y - 5, pos.Z - 5, pos.X + 5, pos.Y + 5, pos.Z + 5, true))
                {
                    Function.Call(Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS, pos.X, pos.Y, pos.Z + 10f, pos.X, pos.Y, pos.Z, 5, true, flare, Game.Player.Character, false, false, 200f);
                    //UI.Notify("Shot flare at "+pos.ToString()+"| Distance: "+pos.DistanceTo(Game.Player.Character.Position));
                }
            }

        }

        static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(new Random().Next(v.Length));
        }

        public static void RandomTuning(Vehicle veh)
        {


            //Change color
            var color = Enum.GetValues(typeof(VehicleColor));
            Random random = new Random();
            veh.PrimaryColor = (VehicleColor)color.GetValue(random.Next(color.Length));

            Random random2 = new Random();
            veh.SecondaryColor = (VehicleColor)color.GetValue(random2.Next(color.Length));

            if (veh.LiveryCount > 0) veh.Livery = GetRandomInt(0, veh.LiveryCount);

            //Change tuning parts
            foreach (int mod in Enum.GetValues(typeof(VehicleMod)).Cast<VehicleMod>())
            {
                if (mod == (int)VehicleMod.Horns) continue;

                veh.SetMod((VehicleMod)mod, Util.GetRandomInt(0, veh.GetModCount((VehicleMod)mod)), false);
            }


            //Change neons if at night
            if (World.CurrentDayTime.Hours > 20 || World.CurrentDayTime.Hours < 7)
            {

                //Color neoncolor = Color.FromArgb(0, Util.GetRandomInt(0, 255), Util.GetRandomInt(0, 255), Util.GetRandomInt(0, 255));

                Color neoncolor = Color.FromKnownColor((KnownColor)Util.GetRandomInt(0, Enum.GetValues(typeof(KnownColor)).Cast<KnownColor>().Count()));
                veh.NeonLightsColor = neoncolor;

                veh.SetNeonLightsOn(VehicleNeonLight.Front, true);
                veh.SetNeonLightsOn(VehicleNeonLight.Back, true);
                veh.SetNeonLightsOn(VehicleNeonLight.Left, true);
                veh.SetNeonLightsOn(VehicleNeonLight.Right, true);

            }


        }


        public static List<string> GetColors()
        {
            //create a generic list of strings
            List<string> colors = new List<string>();
            //get the color names from the Known color enum
            string[] colorNames = Enum.GetNames(typeof(KnownColor));
            //iterate thru each string in the colorNames array
            foreach (string colorName in colorNames)
            {
                //cast the colorName into a KnownColor
                KnownColor knownColor = (KnownColor)Enum.Parse(typeof(KnownColor), colorName);
                //check if the knownColor variable is a System color
                if (knownColor > KnownColor.Transparent)
                {
                    //add it to our list
                    colors.Add(colorName);
                }
            }
            //return the color list
            return colors;
        }
        //Notification Queues

        public static List<String> MessageQueue = new List<String>();
        public static int MessageQueueInterval = 8000;
        public static int MessageQueueReferenceTime = 0;
        public static void HandleMessages()
        {
            if (MessageQueue.Count > 0)
            {
                DisplayHelpTextThisFrame(MessageQueue[0]);
            }
            else
            {
                MessageQueueReferenceTime = Game.GameTime;
            }
            if (Game.GameTime > MessageQueueReferenceTime + MessageQueueInterval)
            {
                if (MessageQueue.Count > 0)
                {
                    MessageQueue.RemoveAt(0);
                }
                MessageQueueReferenceTime = Game.GameTime;
            }
        }
        public static void AddQueuedHelpText(string text)
        {
            if (!MessageQueue.Contains(text)) MessageQueue.Add(text);
        }

        public static void ClearAllHelpText(string text)
        {
            MessageQueue.Clear();
        }


        public static List<String> NotificationQueueText = new List<String>();
        public static List<String> NotificationQueueAvatar = new List<String>();
        public static List<String> NotificationQueueAuthor = new List<String>();
        public static List<String> NotificationQueueTitle = new List<String>();

        public static int NotificationQueueInterval = 8000;
        public static int NotificationQueueReferenceTime = 0;
        public static void HandleNotifications()
        {
            if (Game.GameTime > NotificationQueueReferenceTime)
            {

                if (NotificationQueueAvatar.Count > 0 && NotificationQueueText.Count > 0 && NotificationQueueAuthor.Count > 0 && NotificationQueueTitle.Count > 0)
                {
                    NotificationQueueReferenceTime = Game.GameTime + ((NotificationQueueText[0].Length / 10) * 1000);
                    Notify(NotificationQueueAvatar[0], NotificationQueueAuthor[0], NotificationQueueTitle[0], NotificationQueueText[0]);
                    NotificationQueueText.RemoveAt(0);
                    NotificationQueueAvatar.RemoveAt(0);
                    NotificationQueueAuthor.RemoveAt(0);
                    NotificationQueueTitle.RemoveAt(0);
                }
            }
        }

        public static void AddNotification(string avatar, string author, string title, string text)
        {
            NotificationQueueText.Add(text);
            NotificationQueueAvatar.Add(avatar);
            NotificationQueueAuthor.Add(author);
            NotificationQueueTitle.Add(title);
        }
        public static void CleanNotifications()
        {
            NotificationQueueText.Clear();
            NotificationQueueAvatar.Clear();
            NotificationQueueAuthor.Clear();
            NotificationQueueTitle.Clear();
            NotificationQueueReferenceTime = Game.GameTime;
            Function.Call(Hash._REMOVE_NOTIFICATION, CurrentNotification);
        }

        public static int CurrentNotification;
        public static void Notify(string avatar, string author, string title, string message)
        {
            if (avatar != "" && author != "" && title != "")
            {
                Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
                Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, message);
                CurrentNotification = Function.Call<int>(Hash._SET_NOTIFICATION_MESSAGE, avatar, avatar, true, 0, title, author);
            }
            else
            {
                UI.Notify(message);
            }
        }

    }

}
