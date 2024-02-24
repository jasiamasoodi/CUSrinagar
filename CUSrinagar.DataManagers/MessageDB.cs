using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Models;
using Terex;
using GeneralModels;

namespace CUSrinagar.DataManagers
{
    public class MessageDB
    {
        public List<Message> GetStudentQuickMessageList()
        {

            if (!string.IsNullOrWhiteSpace(AppUserHelper.CollegeCode))
            {
                var columnName = string.IsNullOrWhiteSpace(AppUserHelper.CollegeCode) ? "" : " WHERE [" + AppUserHelper.CollegeCode + "] = 1 ";
                var Query = $@"SELECT TOP 5 Date,MessageTitle FROM Messages {columnName} ORDER BY Date DESC ";
                return new MSSQLFactory().GetObjectList<Message>(Query);
            }
            return null;
        }
        public List<Message> GetStudentMessageList(Parameters parameters)
        {
            if (parameters.Filters == null)
                parameters.Filters = new List<SearchFilter>();
            if (!string.IsNullOrWhiteSpace(AppUserHelper.CollegeCode))
            {
                parameters.Filters.Add(new SearchFilter
                {
                    Column = ($"{AppUserHelper.CollegeCode ?? ""}"),
                    Value = "1"
                });
                var Query = $"SELECT * FROM MESSAGES ";
                var helper = new GeneralFunctions().GetWhereClause<Message>(parameters.Filters);
                Query += helper.WhereClause.Replace("ASC =", " [ASC]=");
                Query += new GeneralFunctions().GetPagedQuery(Query, parameters);
                helper.Command.CommandText = Query;
                return new MSSQLFactory().GetObjectList<Message>(helper.Command);
            }
            return null;
        }
        public int SaveMessage(Message message) => new MSSQLFactory().InsertRecord(message, "Messages");

        public List<Message> GetMessageList(Parameters parameters)
        {
            var Query = "Select * From MESSAGES ";
            var helper = new GeneralFunctions().GetWhereClause<Message>(parameters.Filters);
            Query += helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<Message>(helper.Command);
        }

        public Message Get(Guid id)
        {
            var Query = $"Select * from Messages Where Message_ID='{id}'";
            return new MSSQLFactory().GetObject<Message>(Query);
        }

        public int UpdateMessage(Message message)
        {
            List<string> ignoreList = new List<string>() { "Message_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(message, ignoreList, ignoreList, "Messages");
            sqlCommand.CommandText += " WHERE Message_ID =@Message_ID";
            sqlCommand.Parameters.AddWithValue("@Message_ID", message.Message_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
    }
}
