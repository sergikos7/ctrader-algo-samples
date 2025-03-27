// -------------------------------------------------------------------------------------------------
//
//    This code is a cTrader Algo API example.
//
//    This cBot is intended to be used as a sample and does not guarantee any particular outcome or
//    profit of any kind. Use it at your own risk.
//
//    The Accelerator Oscillator Sample cBot creates a buy order when the Accelerator Oscillator 
//    crosses above zero, indicating positive momentum, and a sell order when it crosses below zero. 
//    Orders are closed by the opposite signal or a stop loss/take profit based on the set parameters. 
//    Only one buy or sell order is allowed at any time.
//
// -------------------------------------------------------------------------------------------------

using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    // Define the cBot attributes, such as TimeZone, AccessRights and its ability to add indicators.
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AddIndicators = true)]
    public class AcceleratorOscillatorSample : Robot
    {
        // Private fields for storing the indicator and trade volume.
        private double _volumeInUnits;  // Store volume in units calculated based on the specified lot size.
        
        private AcceleratorOscillator _acceleratorOscillator;  // Store the Accelerator Oscillator indicator.

        // Define input parameters for the cBot.
        [Parameter("Volume (Lots)", DefaultValue = 0.01)]
        public double VolumeInLots { get; set; }  // Trade volume in lots, with a default of 0.01 lots.

        [Parameter("Stop Loss (Pips)", DefaultValue = 10, MaxValue = 100, MinValue = 1, Step = 1)]
        public double StopLossInPips { get; set; }  // Stop-loss distance in pips, defaulting to 10 pips.

        [Parameter("Take Profit (Pips)", DefaultValue = 10, MaxValue = 100, MinValue = 1, Step = 1)]
        public double TakeProfitInPips { get; set; }  // Take-profit distance in pips, defaulting to 10 pips.

        [Parameter("Label", DefaultValue = "AcceleratorOscillatorSample")]
        public string Label { get; set; }  // Unique label for identifying orders opened by this cBot.

        // This property finds all positions opened by this cBot, filtered by the Label parameter.
        public Position[] BotPositions
        {
            get
            {
                return Positions.FindAll(Label);  // Find positions with the same label used by the cBot.
            }
        }

        // This method is called when the cBot starts and is used for initialisation.
        protected override void OnStart()
        {
            // Convert the specified volume in lots to volume in units for the trading symbol.
            _volumeInUnits = Symbol.QuantityToVolumeInUnits(VolumeInLots);

            // Initialise the Accelerator Oscillator to track momentum and generate signals.
            _acceleratorOscillator = Indicators.AcceleratorOscillator();
        }

        // This method is triggered whenever a bar is closed and drives the decision-making process for the cBot.
        protected override void OnBarClosed()
        {
            // Check existing positions and manage them based on the Accelerator Oscillator's signals.
            foreach (var position in BotPositions)
            {
                // Close buy positions if the oscillator shows a downward signal.
                // Close sell positions if the oscillator shows an upward signal.                
                if ((position.TradeType == TradeType.Buy && _acceleratorOscillator.Result.Last(0) < _acceleratorOscillator.Result.Last(1))
                    || (position.TradeType == TradeType.Sell && _acceleratorOscillator.Result.Last(0) > _acceleratorOscillator.Result.Last(1)))
                {
                    ClosePosition(position);  // Close the position when the opposite signal appears.
                }
            }

            // Evaluate conditions to open new positions based on zero-crossing of the oscillator.

            // If oscillator crosses above zero, a buy signal is generated.
            if (_acceleratorOscillator.Result.Last(0) > 0 && _acceleratorOscillator.Result.Last(1) <= 0)
            {
                ExecuteMarketOrder(TradeType.Buy, SymbolName, _volumeInUnits, Label, StopLossInPips, TakeProfitInPips);  // Open a market order to buy with the specified volume, stop loss and take profit.
            }

            // If oscillator crosses below zero, a sell signal is generated.           
            else if (_acceleratorOscillator.Result.Last(0) < 0 && _acceleratorOscillator.Result.Last(1) >= 0)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, _volumeInUnits, Label, StopLossInPips, TakeProfitInPips);  // Open a market order to sell with the specified volume, stop loss and take profit.
            }
        }
    }
}
