using CUSrinagar.DataManagers;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.BusinessManagers
{
    public class MessageManager
    {
        public List<Message> GetStudentQuickMessageList()
        {
            return new MessageDB().GetStudentQuickMessageList();
        }
        public List<Message> GetStudentMessageList(Parameters parameter)
        {
            return new MessageDB().GetStudentMessageList(parameter);
        }
        public List<Message> GetMessages(Parameters parameters) => new MessageDB().GetMessageList(parameters);

        public ResponseData SaveMessage(Message message)
        {
            var response = new ResponseData();
            message.Message_ID = Guid.NewGuid();
            response.NumberOfRecordsEffected = new MessageDB().SaveMessage(message);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            if (response.IsSuccess) response.SuccessMessage = "Message sent successfully";
            return response;
        }
        public ResponseData UpdateMessage(Message message)
        {
            var response = new ResponseData();
            response.NumberOfRecordsEffected = new MessageDB().UpdateMessage(message);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            if (response.IsSuccess) response.SuccessMessage = "Message sent successfully";
            return response;
        }
        public Message Get(Guid id)
        {
            return new MessageDB().Get(id);
        }
    }
}
