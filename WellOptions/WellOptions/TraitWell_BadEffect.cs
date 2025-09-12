using HarmonyLib;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WellOptions
{
	internal class TraitWell_BadEffect
    {
		public static void DoPatching() {
			Plugin.Logger.LogInfo("DoPatching");
			try {
				//Patch1();
				//Patch2();
				//Patch3();
			} catch (Exception ex) {
				Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}

		/// <summary>
		/// not working
		/// </summary>
		public static void Patch1() {
			var harmony = new Harmony("well.options");
			MethodInfo mOriginal = AccessTools.Method(typeof(TraitWell), nameof(TraitWell.BadEffect)); // if possible use nameof() here
			MethodInfo mPrefix = AccessTools.Method(typeof(TraitWell_BadEffect), nameof(TraitWell_BadEffect.Prefix)); // if possible use nameof() here
			harmony.Patch(mOriginal, new HarmonyMethod(mPrefix));
		}

		/// <summary>
		/// not working
		/// </summary>
		public static void Patch2() {
			MethodInfo methodToReplace = typeof(TraitWell).GetMethod(nameof(TraitWell.BadEffect), BindingFlags.Static | BindingFlags.Public);
			MethodInfo methodToInject = typeof(TraitWell_BadEffect).GetMethod(nameof(TraitWell_BadEffect.BadEffect), BindingFlags.Static | BindingFlags.Public);
			RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
			RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

			unsafe {
				if (IntPtr.Size == 4) {
					int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
					int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
#if DEBUG
					Console.WriteLine("Version x86 Debug");

					byte* injInst = (byte*)*inj;
					byte* tarInst = (byte*)*tar;

					int* injSrc = (int*)(injInst + 1);
					int* tarSrc = (int*)(tarInst + 1);

					*tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
#else
                    Console.WriteLine("\nVersion x86 Release\n");
                    *tar = *inj;
#endif
				} else {

					long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer() + 1;
					long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer() + 1;
#if DEBUG
					Console.WriteLine("Version x64 Debug");
					byte* injInst = (byte*)*inj;
					byte* tarInst = (byte*)*tar;


					int* injSrc = (int*)(injInst + 1);
					int* tarSrc = (int*)(tarInst + 1);

					*tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
#else
                    Console.WriteLine("\nVersion x64 Release\n");
                    *tar = *inj;
#endif
				}
			}
		}

		/// <summary>
		/// not working
		/// </summary>
		public static void Patch3() {
			// get the MethodInfo for the method we're trying to patch 
			// (when the API is public you can use the Expression-based helper; else, try reflection)
			var srcMethod = DetourUtility.MethodInfoForMethodCall(() => TraitWell.BadEffect(default));

			// get the MethodInfo for the replacement method
			var dstMethod = DetourUtility.MethodInfoForMethodCall(() => BadEffect(null));

			// patch the method function pointer
			DetourUtility.TryDetourFromTo(
				src: srcMethod,
				dst: dstMethod
			);
		}

		//private static bool Prefix(ref TraitWell __instance, ref Chara c)
		private bool Prefix(ref TraitWell __instance)
        {
            if (!ModConfig.IsModEnabled.Value)
            {
                return true;
            }
            __instance.ModCharges(+1);
            return false;
        }

		public static void BadEffect(Chara c) {
			Plugin.Logger.LogInfo("Skip BadEffect");
		}
	}

	public static class DetourUtility
	{
		/// <summary> Returns the get accessor MethodInfo obtained from a method call expression. </summary>
		public static MethodInfo MethodInfoForMethodCall(Expression<Action> methodCallExpression)
			=> methodCallExpression.Body is MethodCallExpression { Method: var methodInfo }
				? methodInfo
				: throw new($"Couldn't obtain MethodInfo for the method call expression: {methodCallExpression}");

		/// <summary> Returns the get accessor MethodInfo obtained from a property expression. </summary>
		public static MethodInfo MethodInfoForGetter<T>(Expression<Func<T>> propertyExpression)
			=> propertyExpression.Body is MemberExpression { Member: PropertyInfo { GetMethod: var methodInfo } }
				? methodInfo
				: throw new($"Couldn't obtain MethodInfo for the property get accessor expression: {propertyExpression}");

		/// <summary> Returns the set accessor MethodInfo obtained from a property expression. </summary>
		public static MethodInfo MethodInfoForSetter<T>(Expression<Func<T>> propertyExpression)
			=> propertyExpression.Body is MemberExpression { Member: PropertyInfo { SetMethod: var methodInfo } }
				? methodInfo
				: throw new($"Couldn't obtain MethodInfo for the property set accessor expression: {propertyExpression}");

		// this is based on an interesting technique from the RimWorld ComunityCoreLibrary project, originally credited to RawCode:
		// https://github.com/RimWorldCCLTeam/CommunityCoreLibrary/blob/master/DLL_Project/Classes/Static/Detours.cs
		// licensed under The Unlicense:
		// https://github.com/RimWorldCCLTeam/CommunityCoreLibrary/blob/master/LICENSE
		public static unsafe void TryDetourFromTo(MethodInfo src, MethodInfo dst) {
			try {
				if (IntPtr.Size == sizeof(Int64)) {
					// 64-bit systems use 64-bit absolute address and jumps
					// 12 byte destructive

					// Get function pointers
					long srcBase = src.MethodHandle.GetFunctionPointer().ToInt64();
					long dstBase = dst.MethodHandle.GetFunctionPointer().ToInt64();

					// Native source address
					byte* pointerRawSource = (byte*)srcBase;

					// Pointer to insert jump address into native code
					long* pointerRawAddress = (long*)(pointerRawSource + 0x02);

					// Insert 64-bit absolute jump into native code (address in rax)
					// mov rax, immediate64
					// jmp [rax]
					*(pointerRawSource + 0x00) = 0x48;
					*(pointerRawSource + 0x01) = 0xB8;
					*pointerRawAddress = dstBase; // ( pointerRawSource + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
					*(pointerRawSource + 0x0A) = 0xFF;
					*(pointerRawSource + 0x0B) = 0xE0;
					Plugin.Logger.LogInfo("64 replaced");
				} else {
					// 32-bit systems use 32-bit relative offset and jump
					// 5 byte destructive

					// Get function pointers
					int srcBase = src.MethodHandle.GetFunctionPointer().ToInt32();
					int dstBase = dst.MethodHandle.GetFunctionPointer().ToInt32();

					// Native source address
					byte* pointerRawSource = (byte*)srcBase;

					// Pointer to insert jump address into native code
					int* pointerRawAddress = (int*)(pointerRawSource + 1);

					// Jump offset (less instruction size)
					int offset = dstBase - srcBase - 5;

					// Insert 32-bit relative jump into native code
					*pointerRawSource = 0xE9;
					*pointerRawAddress = offset;
					Plugin.Logger.LogInfo("32 replaced");
				}
			} catch (Exception ex) {
				Console.WriteLine($"Unable to detour: {src?.Name ?? "null src"} -> {dst?.Name ?? "null dst"}\n{ex}");
				throw;
			}
		}
	}
}
