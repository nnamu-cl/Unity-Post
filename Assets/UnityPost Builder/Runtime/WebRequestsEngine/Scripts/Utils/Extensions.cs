    using System.Linq;
    using UnityEngine;

    namespace UnityPost.Utils
    {

        public static class Extensions
        {
            public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
            {
                var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
                var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

                foreach (var sourceProp in sourceProps)
                {
                    if (destProps.Any(x => x.Name == sourceProp.Name))
                    {
                        var destProp = destProps.First(x => x.Name == sourceProp.Name);
                        if (destProp.CanWrite)
                        {
                            destProp.SetValue(dest, sourceProp.GetValue(source, null), null);
                        }
                    }
                }
            }

            public static string ToJSON<T>(this T source)
            {
                return JsonUtility.ToJson(source);
            }

            public static T FromJSON<T>(this string source)
            {
                return JsonUtility.FromJson<T>(source);
            }

        }
    }