using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSend
{
    public class StateHandler<TEnum>
    {
        TEnum _currentState;

        public StateHandler()
        {
            _currentState = (TEnum)Enum.GetValues(typeof(TEnum)).GetValue(0);
            ThrowIfDefaultEnumNull();
        }

        private void ThrowIfDefaultEnumNull()
        {
            if (_currentState is null)
            {
                throw new Exception("StateHandler could not get default value of given enum type.");
            }
        }

        public bool Update(TEnum newState)
        {
            if (_currentState.Equals(newState))
            {
                return false;
            }

            _currentState = newState;

            return true;
        }

        public TEnum GetCurrentState()
        {
            return _currentState;
        }


    }
}
