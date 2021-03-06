﻿using System;
using System.IO;

namespace MobilePhoneRegion
{
    /// <summary>
    /// MobilePhoneFactory
    /// </summary>
    public class MobilePhoneFactory
    {
        private static Lazy<IDataSource> LoadInnerDataSource = new Lazy<IDataSource>(() =>
        {
            var t = typeof(MobilePhoneFactory);
            var name = $"{t.Namespace}.MobilePhoneRegion.dat";

            using (var stream = t.Assembly.GetManifestResourceStream(name))
            {
                return new MemoryDataSource(stream);
            }
        });

        /// <summary>
        /// 生成手机归属地数据源
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="data">手机归属地信息</param>
        /// <param name="stream">要写入的流</param>
        public static void Generate(Version version, MobilePhone[] data, Stream stream)
        {
            switch (version)
            {
                case Version.V1:
                    new Internal.V1.Generator(data).Write(stream);
                    break;
                case Version.V2:
                    new Internal.V2.Generator(data).Write(stream);
                    break;
            }
        }

        /// <summary>
        /// 获取内部数据源
        /// </summary>
        /// <returns>IDataSource</returns>
        public static IDataSource GetInnerDataSource()
        {
            return LoadInnerDataSource.Value;
        }

        /// <summary>
        /// 获取号码归属地查询器
        /// </summary>
        /// <returns><see cref="ISearcher"/></returns>
        /// <exception cref="FileLoadException">Invalid file version</exception>
        public static ISearcher GetSearcher()
        {
            return GetSearcher(GetInnerDataSource());
        }

        /// <summary>
        /// 获取号码归属地查询器
        /// </summary>
        /// <param name="dataSource"><see cref="IDataSource"/></param>
        /// <returns><see cref="ISearcher"/></returns>
        /// <exception cref="FileLoadException">Invalid file version</exception>
        public static ISearcher GetSearcher(IDataSource dataSource)
        {
            var ver = dataSource.ReadByte(0);

            if (Enum.TryParse<Version>(ver.ToString(), out var version))
            {
                switch (version)
                {
                    case Version.V1:
                        return new Internal.V1.Searcher(dataSource);

                    case Version.V2:
                        return new Internal.V2.Searcher(dataSource);
                }
            }

            throw new FileLoadException("Invalid file version");
        }
    }
}