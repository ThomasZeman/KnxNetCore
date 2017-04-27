using System;
using System.Collections.Generic;
using System.Text;
using Amplifier.Units;

namespace KnxNetCore
{
    public class Celsius : IUnit<Celsius>
    {
        public Measure<Celsius> Convert(IMeasure source)
        {
            // Remove this when Amplify is updated
            throw new NotImplementedException();
        }
    }
}
