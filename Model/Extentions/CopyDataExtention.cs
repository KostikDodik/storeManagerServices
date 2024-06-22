using System.Collections;
using System.Reflection;

namespace Model.Extentions
{
    public static partial class ManualInputExtensions
    {
        /// <summary>
        /// Convert DB entity to model
        /// </summary>
        /// <param name="dbEntity"><see cref="IDbEntity"/> class</param>
        /// <typeparam name="TTo">Returning model's type</typeparam>
        /// <returns><see cref="TTo"/> object</returns>
        public static TTo ConvertFromDbEntity<TTo>(this IDbEntity dbEntity)
            where TTo : IModelEntity, new()
        {
            var result = new TTo();
            if (result is ISpecificInfoFromDbEntity specific)
            {
                if (specific.KeepOriginalValues)
                {
                    specific.Fill(dbEntity);
                    result.CopyPossibleProperties(dbEntity);
                }
                else
                {
                    result.CopyPossibleProperties(dbEntity);
                    specific.Fill(dbEntity);
                }
            }
            else
            {
                result.CopyPossibleProperties(dbEntity);
            }
            return result;
        }

        /// <summary>
        /// Convert model entity to db entity
        /// </summary>
        /// <param name="modelEntity"><see cref="IModelEntity"/> object</param>
        /// <typeparam name="TTo">Returning db entity's type</typeparam>
        /// <returns><see cref="TTo"/> object</returns>
        public static TTo ConvertToDbEntity<TTo>(this IModelEntity modelEntity)
            where TTo : IDbEntity, new()
        {
            var result = new TTo().CopyPossibleProperties(modelEntity);
            if (modelEntity is ISpecificInfoToDbEntity specific)
            {
                specific.Pass(result);
            }
            return result;
        }

        /// <summary>
        /// Updates Db entity by model
        /// </summary>
        /// <param name="dbEntity"><see cref="IDbEntity"/> object to update</param>
        /// <param name="modelEntity"><see cref="IModelEntity"/> object with new data</param>
        /// <typeparam name="TTo"><see cref="IDbEntity"/> inheritor</typeparam>
        /// <typeparam name="TFrom"></typeparam>
        /// <returns>Passed <see cref="TTo"/> object with updated data</returns>
        public static TTo UpdateDbEntity<TTo, TFrom>(this TTo dbEntity, TFrom modelEntity)
            where TFrom : class
            where TTo : IDbEntity, new()
        {
            var result = dbEntity.CopyPossibleProperties(modelEntity);
            if (modelEntity is ISpecificInfoToDbEntity specific)
            {
                specific.Pass(result);
            }
            return result;
        }

        /// <summary>
        /// Find and copy all the similar properties between 2 objects
        /// </summary>
        /// <param name="to">Object for copying to </param>
        /// <param name="from">Object for copying from</param>
        /// <returns>Original object with updated data</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TTo CopyPossibleProperties<TTo, TFrom>(this TTo to, TFrom from)
            where TFrom : class
            where TTo : new()
        {
            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            // We are not using typeof because typeof will give us interface
            var fromType = from.GetType();
            var toType = to.GetType();
            // p.CanRead (CanWrite) will return true if getter (setter) exists even if it is inaccessible.
            var fromProps = fromType.GetProperties().Where(prop => prop.GetGetMethod() != null).ToList();
            var toProps = toType.GetProperties().Where(prop => prop.GetSetMethod() != null).ToList();

            if (fromProps.Count > toProps.Count)
            {
                foreach (var setProperty in toProps)
                {
                    var index = fromProps.FindIndex(prop => prop.Name == setProperty.Name);
                    if (index == -1)
                    {
                        continue;
                    }
                    var getProperty = fromProps[index];
                    fromProps.RemoveAt(index);
                    SetPropertyFromGetter(from, to, getProperty, setProperty);
                }
            }
            else
            {
                foreach (var getProperty in fromProps)
                {
                    var index = toProps.FindIndex(prop => prop.Name == getProperty.Name);
                    if (index == -1)
                    {
                        continue;
                    }
                    var setProperty = toProps[index];
                    toProps.RemoveAt(index);
                    SetPropertyFromGetter(from, to, getProperty, setProperty);
                }
            }

            return to;
        }

        /// <summary>
        /// IF possible, copies property value from one generic object to another by provided PropertyInfos
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        private static void SetPropertyFromGetter<TFrom, TTo>(TFrom getObj, TTo setObj, PropertyInfo get, PropertyInfo set)
            where TFrom : class
            where TTo : new()
        {
            if (get.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(get.PropertyType))
            {
                // Avoid copying db rows
                return;
            }
            
            if (getObj == null)
            {
                throw new ArgumentNullException(nameof(getObj));
            }
            if (setObj == null)
            {
                throw new ArgumentNullException(nameof(setObj));
            }

            var setMethod = set?.GetSetMethod();
            var getMethod = get?.GetGetMethod();
            // If we have accessible getter and setter, copy property value.
            if (setMethod == null || getMethod == null)
            {
                return;
            }

            var value = getMethod.Invoke(getObj, null);
            // assign only not null value and if possible
            if (value != null && set.PropertyType.IsAssignableFrom(Nullable.GetUnderlyingType(get.PropertyType) ?? get.PropertyType))
            {
                setMethod.Invoke(setObj, new[] { value });
            }
        }

        /// <summary>
        /// Checks if 2 objects are equal by comparing all properties wth public getters using default Equals method
        /// </summary>
        /// <returns></returns>
        public static bool CompareByPublicFields<T>(this T obj1, T obj2) where T : class
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }
            if (obj2 is null)
            {
                return false;
            }
            var type = typeof(T);
            var publicGetters = type.GetProperties().Select(prop => prop.GetGetMethod()).Where(getter => getter != null).ToList();
            foreach (var getter in publicGetters)
            {
                var value1 = getter.Invoke(obj1, null);
                var value2 = getter.Invoke(obj2, null);
                if (!value1.EnhancedEquals(value2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool EnhancedEquals<T>(this T field1, T field2)
        {
            return field1 == null
                ? field2 == null
                : field2 != null && field1.Equals(field2);
        }
    }
}
