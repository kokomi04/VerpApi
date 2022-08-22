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
        void Mapping(Profile profile) => profile.CreateMapCustom(typeof(T), GetType())
            .ReverseMapCustom(GetType(), typeof(T));
    }
    public interface ICustomMapping
    {
        void Mapping(Profile profile);
    }

    public interface IMapIgnoreNoneExistsPropFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMapCustom(typeof(T), GetType())
            .MapIgnoreNoneExist(typeof(T), GetType())
            .ReverseMap()
            .MapIgnoreNoneExist(GetType(), typeof(T));
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.CreateMapCustom<long, DateTime>().ConvertUsing(v => v.UnixToDateTime().Value);
            this.CreateMapCustom<long?, DateTime?>().ConvertUsing(v => v.UnixToDateTime());

            this.CreateMapCustom<DateTime, long>().ConvertUsing(v => v.GetUnix());
            this.CreateMapCustom<DateTime?, long?>().ConvertUsing(v => v.GetUnix());
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

            var typeIngoreNoneExists = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapIgnoreNoneExistsPropFrom<>)))
                .ToList();

            foreach (var type in typeIngoreNoneExists)
            {
                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod("Mapping")
                    ?? type.GetInterface("IMapIgnoreNoneExistsPropFrom`1").GetMethod("Mapping");

                methodInfo?.Invoke(instance, new object[] { profile });

            }

        }

        public static IMappingExpression CreateMapCustom(this Profile profile, Type sourceType, Type destinationType)
        {
            return profile.CreateMap(sourceType, destinationType);
            //var expression = profile.CreateMap(sourceType, destinationType);
            //return MapIgnoreNoneExist(expression, sourceType, destinationType);
        }


        public static IMappingExpression<ISource, IDestination> CreateMapCustom<ISource, IDestination>(this Profile profile)
        {
            return profile.CreateMap<ISource, IDestination>();
            //var expression = profile.CreateMap<ISource, IDestination>();
            //return MapIgnoreNoneExist(expression);
        }


        public static IMappingExpression ReverseMapCustom(this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            return expression.ReverseMap();
            //expression = expression.ReverseMap();
            //return MapIgnoreNoneExist(expression, sourceType, destinationType);
        }

        public static IMappingExpression<IDestination, ISource> ReverseMapCustom<ISource, IDestination>(this IMappingExpression<ISource, IDestination> expression)
        {
            return expression.ReverseMap();
            //var reverseExpression = expression.ReverseMap();
            //return MapIgnoreNoneExist(reverseExpression);
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

        public static IMappingExpression<ISource, IDestination> IgnoreNoneExist<ISource, IDestination>(this IMappingExpression<ISource, IDestination> expression)
        {
            var sourceType = typeof(ISource);
            var destinationType = typeof(IDestination);

            if (!sourceType.IsClass) return expression;
            if (!destinationType.IsClass) return expression;

            var sourceProps = sourceType.GetProperties();

            var desProps = destinationType.GetProperties();

            expression.ForAllOtherMembers(opts =>
            {
                if (!sourceProps.Any(d => d.Name == opts.DestinationMember.Name))
                    opts.Ignore();
            });

            return expression;
        }

    }
}
