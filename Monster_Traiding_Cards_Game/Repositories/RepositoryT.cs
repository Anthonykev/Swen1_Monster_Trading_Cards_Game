using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Npgsql;
using Monster_Traiding_Cards.Base;

namespace Monster_Traiding_Cards.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : IAtom, new()
    {
        private static IDbConnection? _DbConnection = null;

        protected string _TableName = string.Empty;
        protected string[] _Fields = Array.Empty<string>();
        protected string[] _Params = Array.Empty<string>();

        protected static IDbConnection _Cn
        {
            get
            {
                if (_DbConnection == null)
                {
                    _DbConnection = new NpgsqlConnection("Host=localhost;Username=kevin;Password=spiel12345;Database=monster_cards");
                    _DbConnection.Open();
                }

                return _DbConnection;
            }
        }

        protected virtual T _CreateObject(IDataReader re)
        {
            T rval = new();
            ((__IAtom)rval).__InternalID = re.GetInt32(0);
            return _RefeshObject(re, rval);
        }

        protected abstract T _RefeshObject(IDataReader re, T obj);

        protected abstract void _FillParameters(IDbCommand cmd, T obj);

        public virtual T Get(object id)
        {
            using (IDbCommand cmd = _Cn.CreateCommand())
            {
                cmd.CommandText = $"SELECT {string.Join(", ", _Fields)} FROM {_TableName} WHERE {_Fields[0]} = :id";
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = ":id";
                p.Value = id;
                cmd.Parameters.Add(p);

                using (IDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        return _CreateObject(re);
                    }
                }
            }

            throw new DataException("No such object.");
        }

        public virtual IEnumerable<T> GetAll()
        {
            List<T> rval = new();

            using (IDbCommand cmd = _Cn.CreateCommand())
            {
                cmd.CommandText = $"SELECT {string.Join(", ", _Fields)} FROM {_TableName}";

                using (IDataReader re = cmd.ExecuteReader())
                {
                    while (re.Read())
                    {
                        rval.Add(_CreateObject(re));
                    }
                }
            }

            return rval;
        }

        public virtual void Refresh(T obj)
        {
            using (IDbCommand cmd = _Cn.CreateCommand())
            {
                cmd.CommandText = $"SELECT {string.Join(", ", _Fields)} FROM {_TableName} WHERE {_Fields[0]} = :id";
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = ":id";
                p.Value = obj.__InternalID;
                cmd.Parameters.Add(p);

                using (IDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        _RefeshObject(re, obj);
                    }
                }
            }
        }

        public virtual void Save(T obj)
        {
            if (obj.__InternalID is null)
            {
                using (IDbCommand cmd = _Cn.CreateCommand())
                {
                    cmd.CommandText = $"INSERT INTO {_TableName} ({string.Join(", ", _Fields.Skip(1))}) VALUES ({string.join(", ", _Params.Skip(1))}) RETURNING {_Fields[0]}";
                    _FillParameters(cmd, obj);
                    obj.__InternalID = Convert.ToInt32(cmd.ExecuteScalar()!);
                }
            }
            else
            {
                using (IDbCommand cmd = _Cn.CreateCommand())
                {
                    cmd.CommandText = $"UPDATE {_TableName} SET ";
                    for (int i = 1; i < _Fields.Length; i++)
                    {
                        if (i > 1) { cmd.CommandText += ", "; }
                        cmd.CommandText += (_Fields[i] + " = " + _Params[i]);
                    }
                    cmd.CommandText += $" WHERE {_Fields[0]} = :id";

                    _FillParameters(cmd, obj);
                    IDataParameter p = cmd.CreateParameter();
                    p.ParameterName = ":id";
                    p.Value = obj.__InternalID;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public virtual void Delete(T obj)
        {
            using (IDbCommand cmd = _Cn.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM {_TableName} WHERE {_Fields[0]} = :id";
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = ":id";
                p.Value = obj.__InternalID;
                cmd.Parameters.Add(p);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
