using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace ParserBot
{
    public class StateManager
    {
        private readonly ConcurrentDictionary<long, string> _userState = new ConcurrentDictionary<long, string>();
        private readonly ConcurrentDictionary<long, List<string>> _selectedCategories = new();
        private readonly ConcurrentDictionary<long, int> _categoryMessageIds = new();


        public List<string> GetSelectedCategories(long chatId)
        {
            var list = _selectedCategories.GetOrAdd(chatId, _ => new List<string>());
            return list;
        }

        public void ToggleCategory(long chatId, string categoryName)
        {
            var list = _selectedCategories.GetOrAdd(chatId, _ => new List<string>());
            if(list.Contains(categoryName))
            {
                list.Remove(categoryName);
            }
            else
            {
                list.Add(categoryName);
            }
        }

        public void ClearSelectedCategories(long chatId)
        {
            _selectedCategories.TryRemove(chatId, out _);
        }



        public void SetCategoryMessageId(long chatId, int messageId)
        {
            _categoryMessageIds[chatId] = messageId;
        }
        public int? GetCategoryMessageId(long chatId)
        {
            if (_categoryMessageIds.TryGetValue(chatId, out var id))
            {
                return id;
            }
            else
            {
                return null;
            }
        }



        public void SetUserState(long chatId, string state)
        {
            _userState[chatId] = state;
        }

        public string? GetUserState(long chatId)
        {
            if (_userState.TryGetValue(chatId, out var state))
            {
                return state;
            }
            else
            {
                return null;
            }
        }
        public void RemoveUserState(long chatId)
        {
            _userState.TryRemove(chatId, out _);
        }
    }
}
