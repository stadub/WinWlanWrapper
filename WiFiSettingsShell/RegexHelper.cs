using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.Mappers;

namespace WiFiSettingsShell
{
    internal class RegexHelper
    {
        static RegexHelper()
        {
            Mapper.CreateMap<string, int>().ConvertUsing(Convert.ToInt32);

            Mapper.CreateMap<string, AuthAlgorithm>().ConvertUsing(s =>
            {
                AuthAlgorithm authAlgorithm;
                Enum.TryParse(s.Split('-')[0],true, out authAlgorithm);
                return authAlgorithm;
            });
            Mapper.CreateMap<string,IEnumerable<Interface>>()
                  .ConvertUsing(new RegexTypeConverter<Interface>(InterfacesRegex));

            Mapper.CreateMap<string, IEnumerable<WlanNetworkInfo>>()
                  .ConvertUsing(new RegexTypeConverter<WlanNetworkInfo>(AvalibleNetworksRegex));


            //var bssIdMapper = Mapper.CreateMap<string, BssId>();
            //bssIdMapper.ConvertUsing(new RegexStringTypeConverter<BssId>(BssidRegex));
            
            var bssIdsMapper = Mapper.CreateMap<CaptureCollection, List<BssId>>();
            bssIdsMapper.ConvertUsing(new RegexTypeListConverter<BssId>(BssidRegex));
        }

        private static readonly Regex InterfacesRegex = new Regex(
              "\\s*Name\\s+:\\s*(?<Name>.*)\r\n" +
              "\\s*Description\\s+:\\s*(?<Description>.*)\r\n"+
                "\\s*GUID\\s+:\\s*(?<Guid>.*)\r\n"+
                "\\s*Physical address\\s+:\\s*(?<PhysicalAddress>.*)\r\n"+
                "\\s*State\\s+:\\s*(?<State>.*)\r\n"
              ,RegexOptions.ECMAScript | RegexOptions.Compiled);

        private static readonly Regex AvalibleNetworksRegex = new Regex(
            "^SSID (?<Index>\\d+)\\s*: (?<Name>.*)\r\n" +
            "\\s*Network type\\s*: (?<NetworkType>.*)\r\n" +
            "\\s*Authentication\\s*: (?<AuthenticationType>.*)\r\n" +
            "\\s*Encryption\\s*:\\s*(?<EncryptionType>\\w*)\\s*\r\n" +
            "(?<BssIds>" +
            "\\s*BSSID.*\r\n" +
            "\\s*Signal" +
            "\\s*.* \r\n" +
            "\\s*Radio type" +
            "\\s*.*\r\n" +
            "\\s*Channel\\s*: .*\r\n" +
            "\\s*Basic rates \\(Mbps\\).*\r\n" +
            "(\\s*Other rates \\(Mbps\\).*)?" +
            ")+",RegexOptions.Multiline| RegexOptions.ECMAScript| RegexOptions.Compiled);

        private static readonly Regex BssidRegex = new Regex(
            "(?<BSSID>\\s*BSSID (?<BssIndex>\\d+)\\s*: (?<Mac>.*)\r\n" +
            "\\s*Signal\\s*: (?<SignalStreich>\\d+)%\\s*\r\n" +
            "\\s*Radio type\\s*: (?<RadioType>.*)\r\n" +
            "\\s*Channel\\s*: (?<Channel>\\d+) \r\n" +
            "\\s*Basic rates \\(Mbps\\) : (?<Rates>.*)\\s*\r\n" +
            "(\\s*Other rates \\(Mbps\\) : (?<OtherRates>[\\w|\\s]*)\\s*)?)",
            RegexOptions.ECMAScript| RegexOptions.Compiled);

        public IEnumerable<Interface> GetInterfaces(string interfacesString)
        {
            return Mapper.Map<string, IEnumerable<Interface>>(interfacesString);
        }

        public IEnumerable<WlanNetworkInfo> GetAvalibleNetworks(string avalibleNetworkString)
        {
            return Mapper.Map<string, IEnumerable<WlanNetworkInfo>>(avalibleNetworkString);
        }
    }

    public class RegexListTypeConverter<T> : RegexTypeConverter<T>, ITypeConverter<string, List<IEnumerable<T>>> where T : new()
    {
        public RegexListTypeConverter(Regex regex) : base(regex)
        {
        }

        protected RegexListTypeConverter(Regex regex, IMappingExpression<Match, T> mapping) : base(regex, mapping)
        {
        }

        public new List<IEnumerable<T>> Convert(ResolutionContext context)
        {
            return new List<IEnumerable<T>>();
        }
    }

    public class RegexTypeConverter<T> : ITypeConverter<string, IEnumerable<T>> where T : new()
    {
        private readonly Regex regex;

        public RegexTypeConverter(Regex regex)
            : this(regex, Mapper.CreateMap<Match, T>())
        {
        }

        protected RegexTypeConverter(Regex regex,IMappingExpression<Match,T> mapping)
        {
            this.regex = regex;
            mapping.ConvertUsing(new RegexMatchTypeConverter<T>(regex));
        }

        public virtual IEnumerable<T> Convert(ResolutionContext context)
        {
            var value = context.SourceValue as string;
            if (value == null)
                yield break;

            var matches=regex.Matches(value);
            foreach (Match match in matches)
            {
                yield return Mapper.Map<Match, T>(match);
            }
        }
    }
    public class RegexTypeListConverter<T> : ITypeConverter<CaptureCollection, List<T>> where T : new()
    {
        private readonly Regex regex;

        public RegexTypeListConverter(Regex regex)
            : this(regex, Mapper.CreateMap<Match, T>())
        {
        }

        protected RegexTypeListConverter(Regex regex, IMappingExpression<Match, T> mapping)
        {
            this.regex = regex;
            mapping.ConvertUsing(new RegexMatchTypeConverter<T>(regex));
        }

        public virtual List<T> Convert(ResolutionContext context)
        {
            var captureCollection = context.SourceValue as CaptureCollection;
            var results = new List<T>();
            if (captureCollection == null)
                return results;
            for (int i = 0; i < captureCollection.Count; i++)
            {
                var matches = regex.Matches(captureCollection[i].Value);
                foreach (Match match in matches)
                {
                    results.Add(Mapper.Map<Match, T>(match));
                }
            }

            return results;
        }
    }

    public class RegexStringTypeConverter<T> : ITypeConverter<string, T> where T : new()
    {
        private readonly Regex regex;

        public RegexStringTypeConverter(Regex regex)
            : this(regex, Mapper.CreateMap<Match, T>())
        {
        }

        protected RegexStringTypeConverter(Regex regex, IMappingExpression<Match, T> mapping)
        {
            this.regex = regex;
            mapping.ConvertUsing(new RegexMatchTypeConverter<T>(regex));
        }

        public T Convert(ResolutionContext context)
        {
            var value = context.SourceValue as string;
            if (value == null)
                return default(T);
            var match = regex.Match(value);
            return Mapper.Map<Match, T>(match);
        }
    }


    public class RegexMatchTypeConverter<T> : ITypeConverter<Match, T> where T : new()
    {
        private readonly Regex regex;

        public RegexMatchTypeConverter(Regex regex)
        {
            this.regex = regex;
        }

        public T Convert(ResolutionContext context)
        {
            var match = context.SourceValue as Match;
            if (match == null)
            {
                Trace.TraceWarning("Match is empty or type incorrect");
                return default(T);
            }
            var result = new T();
            var retultType = typeof (T);
            var groups = match.Groups;
            for (int i = 1; i < groups.Count; i++)
            {
                var groupName = regex.GroupNameFromNumber(i);
                if (!groups[i].Success)
                {
                    Trace.TraceWarning("Group {0} value is received.", groupName);
                    continue;
                }

                var propInfo = retultType.GetProperty(groupName);
                if (propInfo == null)
                {
                    Trace.TraceWarning("Public property matching to group '{0}' is not found.", groupName);
                    continue;
                }
                CaptureCollection captures = groups[i].Captures;
                switch (GetPropertyType(propInfo))
                {
                    case PropertyType.String:
                        MapStringValue(result, captures, propInfo);
                        break;
                    case PropertyType.Collection:
                        MapValueCollection(result, captures, propInfo);
                        break;
                    case PropertyType.Other:
                        MapValue(result, captures, propInfo);
                        break;
                }
            }

            return result;
        }

        protected void MapValue(T dest,CaptureCollection captures, PropertyInfo propInfo)
        {
            Debug.Assert(captures.Count <= 1, "Only one capture per group is alowed");
            if (captures.Count == 0)
                return;
            var value = Mapper.Map(captures[0].Value, typeof(string), propInfo.PropertyType);
            propInfo.SetValue(dest, value);
        }

        protected void MapStringValue(T dest,CaptureCollection captures, PropertyInfo propInfo)
        {
            Debug.Assert(captures.Count <= 1, "Only one capture per group is alowed");
            var capture = captures.Count > 0 ? captures[0].Value : string.Empty;
            propInfo.SetValue(dest, capture);
        }

        protected void MapValueCollection(T dest, CaptureCollection captures, PropertyInfo propInfo)
        {
            var value = Mapper.Map(captures, typeof(CaptureCollection), propInfo.PropertyType);
            propInfo.SetValue(dest, value);
        }

        protected virtual PropertyType GetPropertyType(PropertyInfo propInfo)
        {
            if (propInfo.PropertyType == typeof(string))
                return PropertyType.String;
            if (typeof (IEnumerable).IsAssignableFrom(propInfo.PropertyType))
                return PropertyType.Collection;
            return PropertyType.Other;
        }

        protected enum PropertyType
        {
            String,Collection,Other
        }
    }

}
