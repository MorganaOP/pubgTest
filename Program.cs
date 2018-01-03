using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

// uword 48 8B 1D ?? ?? ?? ?? ?? ?? ?? 10 4C 8D 4D ?? 4C  |SAME
namespace PUBGTEST
{
    static class Program
    {
        // COPY PROJECT https://github.com/G-E-N-E-S-I-S/TslGame_MULTI_HACK
        [STAThread]
        static void Main()
        {
            //DriverExploits > https://github.com/G-E-N-E-S-I-S/loadlibrayy
            Process[] proc = Process.GetProcessesByName("tslgame");
            if (proc.Length <= 0) 
                return; // !rungame

            if (DriverExploits.ElevateHandle.Driver.Load())
            {
                DriverExploits.ElevateHandle.UpdateDynamicData();
                if (DriverExploits.ElevateHandle.g_SectionBaseAddress == 0)
                {
                    Console.WriteLine("Update g_SectionBaseAddress");
                    return;
                }

                Loadlibrayy.Natives.NT.ProcessContext procInfo = DriverExploits.ElevateHandle.FindProcessInfo((uint)proc[0].Id);

                ulong pUWorld = 0;
                ulong pGameInstance = 0;
                ulong pLocalPlayerArray = 0;
                ulong pLocalPlayer = 0;
                ulong pViewportClient = 0;
                ulong pPlayerController = 0;
                ulong pLocalPawn = 0;
                ulong pPlayerCameraManager = 0;
                ulong pPersistentLevel = 0;
                ulong pGNames = 0;
                Vector3 localPos = new Vector3(0, 0, 0);

                UInt32 pGNamesOffset = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<UInt32>(procInfo.DirectoryBase, G.procInfo.SectionBaseAddress + 0x9CA703 + 3);
                UInt32 pUWorldOffset = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<UInt32>(procInfo.DirectoryBase, G.procInfo.SectionBaseAddress + 0x1658229 + 3);

                pGNames = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, G.procInfo.SectionBaseAddress + pGNamesOffset + 0x9CA703 + 3 + 4);
                pUWorld = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, G.procInfo.SectionBaseAddress + pUWorldOffset + 0x1658229 + 3 + 4);
                if (pUWorld > 0)
                {
                    pGameInstance = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pUWorld + 0x140);
                    if (pGameInstance > 0)
                    {
                        pLocalPlayerArray = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pGameInstance + 0x38);
                        if (pLocalPlayerArray > 0)
                        {
                            pLocalPlayer = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pLocalPlayerArray + 0x0);
                            if (pLocalPlayer > 0)
                            {
                                pPlayerController = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pLocalPlayer + 0x30);
                                pViewportClient = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pLocalPlayer + 0x58);
                                if (pPlayerController > 0)
                                {
                                    pPlayerCameraManager = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pPlayerController + 0x0448);
                                    if (pPlayerCameraManager > 0)
                                    {
                                        //Vector3 CameraInfoPos = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<Vector3>(info.DirectoryBase, pPlayerCameraManager + 0x010);
                                        //Console.WriteLine("X: " + CameraInfoPos.X.ToString("F") + " ,Y: " + CameraInfoPos.Y.ToString("F") + " ,Z: " + CameraInfoPos.Z.ToString("F"));
                                    }
                                    pLocalPawn = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pPlayerController + 0x0428);
                                    if (pLocalPawn > 0)
                                        localPos = GetActorPos(procInfo.DirectoryBase, pLocalPawn);
                                }
                            }
                        }
                    } // if (pGameInstance > 0)

                    pPersistentLevel = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pUWorld + 0x30);
                    if (pPersistentLevel > 0)
                    {
                        FMinimalViewInfo CameraInfo = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<FMinimalViewInfo>(procInfo.DirectoryBase, pPlayerCameraManager + 0x0420 + 0x10);
                        uint count = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<uint>(procInfo.DirectoryBase, pPersistentLevel + 0xB8);
                        ulong enlist = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, pPersistentLevel + 0xB0);
                        for (int i = 0; i < count; i++)
                        {
                            ulong entity = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, enlist + (ulong)(i * 0x8));
                            if (entity > 0 && entity != pLocalPawn)
                            {
                                int id = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<int>(procInfo.DirectoryBase, entity + 0x18);
                                string objName = GetGNames(procInfo.DirectoryBase, pGNames, id);
                                if (!IsPlayer(objName))
                                    continue;

                                float health = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<float>(procInfo.DirectoryBase, entity + 0x113C);
                                //float Maxhealth = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<float>(info.DirectoryBase, entity + 0x113C + 4);
                                if (health <= 0)
                                    continue;

                                ulong team = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(procInfo.DirectoryBase, entity + 0x0CF8);
                                if (team > 0)
                                    continue;

                                if (pPlayerCameraManager > 0)
                                {
                                    //float BaseEyeHeight = GetBaseEyeHeight(procInfo.DirectoryBase, entity);
                                    Vector3 actorPos = GetActorPos(procInfo.DirectoryBase, entity);
                                    Vector3 vecRelativePos = localPos - actorPos;
                                    double lDeltaInMeters = vecRelativePos.Length / 100;

                                    if (lDeltaInMeters > 1000.0f)
                                        continue;
                                    Vector2 screenlocation;
                                    if (WorldToScreen(actorPos, CameraInfo, out screenlocation))
                                    {
                                        //DrawHealth(screenlocation.X, screenlocation.Y);
                                    }

                                    Console.WriteLine("objName: " + objName + ", Distance: " + lDeltaInMeters + ", Health:" + health);
                                }
                            }
                        }
                    }
               }

                Console.WriteLine("pUWorld" + string.Format("0x{0:X}", pUWorld));
                Console.WriteLine("pGameInstance" + string.Format("0x{0:X}", pGameInstance));
                Console.WriteLine("pLocalPlayerArray" + string.Format("0x{0:X}", pLocalPlayerArray));
                Console.WriteLine("pLocalPlayer" + string.Format("0x{0:X}", pLocalPlayer));
                Console.WriteLine("pViewportClient" + string.Format("0x{0:X}", pViewportClient));
                Console.ReadLine();
                DriverExploits.ElevateHandle.Driver.Unload();
            }
        }

        //https://github.com/HKbool/Pubg_d2d_code/tree/master/Win32Project1
        public static string GetGNames(ulong DirectoryBase, ulong GNamesAddr, int _id)
        {
            int IdDiv = 0, Idtemp = 0;

            if (_id < 0 || _id > 200000)
                return string.Empty;

            IdDiv = _id / 0x4000;
            Idtemp = _id % 0x4000;
            ulong fNamePtr = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(DirectoryBase, GNamesAddr + (ulong)IdDiv * 8);
            ulong fName = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(DirectoryBase, fNamePtr + 8 * (ulong)Idtemp);
            fName = fName + 0x10;
            return DriverExploits.ElevateHandle.Driver.ReadPhysicalString(DirectoryBase, fName);
        }

        // https://github.com/HKbool/Pubg_d2d_code/tree/master/Win32Project1
        public static bool IsPlayer(string name)
        {
            string StringFitter3 = "PlayerMale_A PlayerMale_A_C PlayerFemale_A PlayerFemale_A_C";
            return StringFitter3.Contains(name);
        }

        //https://github.com/HKbool/Pubg_d2d_code/tree/master/Win32Project1
        public static Vector3 GetActorPos(ulong DirectoryBase, ulong AcctorAddr)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            // Class Engine.SceneComponent
            ulong pRootComponent = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<ulong>(DirectoryBase, AcctorAddr + 0x0188);
            if (pRootComponent > 0)
            {
                pos = DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<Vector3>(DirectoryBase, pRootComponent + 0x02D0);
            }
            return pos;
        }

        public static float GetBaseEyeHeight(ulong DirectoryBase, ulong AcctorAddr)
        {
            return DriverExploits.ElevateHandle.Driver.ReadPhysicalAddress<float>(DirectoryBase, AcctorAddr + 0x03BC);
        }

        public static bool WorldToScreen(Vector3 WorldLocation, FMinimalViewInfo CameraInfo, out Vector2 Screenlocation)
        {
            Screenlocation = new Vector2(0, 0);

            FRotator Rotation = CameraInfo.Rotation;

            Vector3 vAxisX, vAxisY, vAxisZ;
            Rotation.GetAxes(out vAxisX, out vAxisY, out vAxisZ);

            Vector3 vDelta = WorldLocation - CameraInfo.Location;
            Vector3 vTransformed = new Vector3(Vector3.DotProduct(vDelta, vAxisY), Vector3.DotProduct(vDelta, vAxisZ), Vector3.DotProduct(vDelta, vAxisX));

            if (vTransformed.Z < 1f)
                vTransformed.Z = 1f;

            float FovAngle = CameraInfo.FOV;
            float ScreenCenterX = 1440 / 2;
            float ScreenCenterY = 900 / 2;

            Screenlocation.X = ScreenCenterX + vTransformed.X * (ScreenCenterX / (float)Math.Tan(FovAngle * (float)Math.PI / 360)) / vTransformed.Z;
            Screenlocation.Y = ScreenCenterY - vTransformed.Y * (ScreenCenterX / (float)Math.Tan(FovAngle * (float)Math.PI / 360)) / vTransformed.Z;

            return true;
        }
    }
}
