using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;

namespace WebApplication1 {
    //https://www.donet5.com/Home/Doc
    public class SqlHelper<T> where T : class, new()
    {
        public bool ExecuteSql(string sql) {
            try {
                var code = GetInstance().Ado.ExecuteCommand(sql);
                return code > 1;
            }
            catch (Exception ex) {
                return false;
            }
        }
       
        public bool Update(T model, string[] cols) {
            var db = GetInstance();
            return db.Updateable<T>(model).UpdateColumns(cols).ExecuteCommand() > 0;
        }
        public List<T> GetList(Expression<Func<T, bool>> where = null) {
            var db = GetInstance();
            if (where == null) {
                return db.Queryable<T>().ToList();
            }
            else {
                return db.Queryable<T>().Where(where).ToList();
            }
        }
        public T Get(Expression<Func<T, bool>> where, Expression<Func<T, object>> order, bool asc = false) {
            SqlSugarClient db = GetInstance();
            if (order == null) {
                return db.Queryable<T>().Where(where).Single();
            }
            else {
                return db.Queryable<T>().Where(where).OrderBy(order, asc ? OrderByType.Asc : OrderByType.Desc).First();
            }
        }
        public T Get(Expression<Func<T, bool>> where, Expression<Func<T, object>> orderBy = null) {
                T model = null;
            try {
                var db = GetInstance();
                if (orderBy != null) {
                    model = db.Queryable<T>().Where(where).OrderBy(orderBy, OrderByType.Desc).First();//查询表的所有
                }
                else {
                    model = db.Queryable<T>().Where(where).First();
                }
                return model;
            }
            catch (Exception ex) {
                //LogHelper.Info(ex.Message);
                return default(T);
            }
        }
        public bool Update(T model) {
            var db = GetInstance();
            return db.Updateable<T>(model).ExecuteCommand() > 0;
        }
        public bool Insert(T model) {
            try {
                var db = GetInstance();
                return db.Insertable<T>(model).ExecuteCommand() > 0;
            }
            catch (Exception ex) {
                return false;
            }
        }

        //删除指定条件数据
        public bool Deleteable(Expression<Func<T, bool>> where) {
            try {
                var db = GetInstance();
                return db.Deleteable<T>().Where(where).ExecuteCommand() > 0;
            }
            catch (Exception ex) {
                return false;
            }
        }

        public bool CreateTable()
        {
            bool result = false;
            var db = GetInstance();

            //定义SQL语句
            //string sql = "create table student" + "(num varchar(50),name varchar(255),age int)";
            
            try
            {

                // 创建数据库
                //db.Context.DbMaintenance.CreateDatabase();
                // 创建表
                db.Context.CodeFirst.InitTables(
                    typeof(T)
                );
                ////LogHelper.Info("数据表创建成功。");
            }
            catch (Exception ae)
            {
                //LogHelper.Info("数据表创建失败。" + ae.Message.ToString());
            }
            finally
            {
                //db.Close();//对数据库操作完成后，需要关闭数据库，释放内存
            }
            return result;
        }


        //创建SqlSugarClient 
        public SqlSugarClient GetInstance() {
            //创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig() {
                ConnectionString = ConfigHelper.DefaultConnectionString,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute//从特性读取主键自增信息
            });
            //db.MappingTables.Add("","");

            //监控所有超过1秒的Sql
            db.Aop.OnLogExecuted = (sql, p) => {
                //LogHelper.Info(sql + "\r\n" + db.Utilities.SerializeObject(p.ToDictionary(it => it.ParameterName, it => it.Value)));
                //执行时间超过1秒
                ////LogHelper.Info(sql);
                if (db.Ado.SqlExecutionTime.TotalSeconds > 1) {
                    //代码CS文件名
                    var fileName = db.Ado.SqlStackTrace.FirstFileName;
                    //代码行数
                    var fileLine = db.Ado.SqlStackTrace.FirstLine;
                    //方法名
                    var FirstMethodName = db.Ado.SqlStackTrace.FirstMethodName;
                    //db.Ado.SqlStackTrace.MyStackTraceList[1].xxx 获取上层方法的信息
                }
                //相当于EF的 PrintToMiniProfiler
            };
            //据转换 (ExecuteCommand才会拦截，查询不行)
            db.Aop.DataExecuted = (value, entity) =>
            {
                //只有行级事件
                entity.EntityColumnInfos.ToList().ForEach(a =>
                {
                    var pvalue = entity.GetValue(a.PropertyName);
                    if (pvalue != null && pvalue.GetType() == typeof(String))
                    {
                        entity.SetValue(a.PropertyName, pvalue.ToString().Trim());
                    }
                });
            };

            return db;
        }
    }
}
