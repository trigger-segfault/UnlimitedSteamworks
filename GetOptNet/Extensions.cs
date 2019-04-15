using System.Linq;
using System.Reflection;

namespace NMaier.GetOptNet {
	internal static class Extensions {
		internal static T GetAttribute<T>(this MemberInfo info) {
			return info.GetAttributes<T>().First();
		}

		internal static T[] GetAttributes<T>(this MemberInfo info) {
			return info.GetCustomAttributes(typeof(T), true) as T[] ?? new T[0];
		}

		internal static bool HasAttribute<T>(this MemberInfo info) {
			object[] attr = info.GetCustomAttributes(typeof(T), true);
			return attr.Length != 0;
		}
	}
}
