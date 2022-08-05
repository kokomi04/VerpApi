using AutoMapper;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace VErp.Commons.GlobalObject
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMapIgnoreNoneExist(typeof(T), GetType())
            .ReverseMapIgnoreNoneExist(GetType(), typeof(T));
    }
    public interface ICustomMapping
    {
        void Mapping(Profile profile);
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.CreateMapIgnoreNoneExist<long, DateTime>().ConvertUsing(v => v.UnixToDateTime().Value);
            this.CreateMapIgnoreNoneExist<long?, DateTime?>().ConvertUsing(v => v.UnixToDateTime());

            this.CreateMapIgnoreNoneExist<DateTime, long>().ConvertUsing(v => v.GetUnix());
            this.CreateMapIgnoreNoneExist<DateTime?, long?>().ConvertUsing(v => v.GetUnix());
        }
    }

    public static class MappingProfileExtension
    {
        public static void ApplyMappingsFromAssembly(this Profile profile, Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod("Mapping")
                    ?? type.GetInterface("IMapFrom`1").GetMethod("Mapping");

                methodInfo?.Invoke(instance, new object[] { profile });

            }

            var customs = assembly.GetExportedTypes()
               .Where(t => t.GetInterfaces().Any(i => i == typeof(ICustomMapping)))
               .ToList();

            foreach (var type in customs)
            {
                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod("Mapping");

                methodInfo?.Invoke(instance, new object[] { profile });

            }
        }

        public static IMappingExpression CreateMapIgnoreNoneExist(this Profile profile, Type sourceType, Type destinationType)
        {
            var expression = profile.CreateMap(sourceType, destinationType);
            return MapIgnoreNoneExist(expression, sourceType, destinationType);
        }


        public static IMappingExpression<ISource, IDestination> CreateMapIgnoreNoneExist<ISource, IDestination>(this Profile profile)
        {
            var expression = profile.CreateMap<ISource, IDestination>();
            return MapIgnoreNoneExist(expression);
        }


        public static IMappingExpression ReverseMapIgnoreNoneExist(this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            expression = expression.ReverseMap();
            return MapIgnoreNoneExist(expression, sourceType, destinationType);
        }

        public static IMappingExpression<IDestination, ISource> ReverseMapIgnoreNoneExist<ISource, IDestination>(this IMappingExpression<ISource, IDestination> expression)
        {
            var reverseExpression = expression.ReverseMap();
            return MapIgnoreNoneExist(reverseExpression);
        }


        public static IMappingExpression MapIgnoreNoneExist(this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            if (!sourceType.IsClass) return expression;
            if (!destinationType.IsClass) return expression;

            var sourceProps = sourceType.GetProperties();

            var desProps = destinationType.GetProperties();

            foreach (var property in desProps)
            {
                if (!sourceProps.Any(d => d.Name == property.Name))
                    expression.ForMember(property.Name, s => s.Ignore());
            }

            return expression;
        }

        public static IMappingExpression<ISource, IDestination> MapIgnoreNoneExist<ISource, IDestination>(this IMappingExpression<ISource, IDestination> expression)
        {
            var sourceType = typeof(ISource);
            var destinationType = typeof(IDestination);


            if (!sourceType.IsClass) return expression;
            if (!destinationType.IsClass) return expression;

            var sourceProps = sourceType.GetProperties();

            var desProps = destinationType.GetProperties();

            foreach (var property in desProps)
            {
                if (!sourceProps.Any(d => d.Name == property.Name))
                    expression.ForMember(property.Name, s => s.Ignore());
            }

            return expression;
        }

    }
}
