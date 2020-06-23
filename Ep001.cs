//==============================================================================
// Project:     	The Trade Risk - Beyond The Charts Episode 001 	           
// Name:        	EP001 													   
// Description: 	Is trading the golden cross 50/200 SMA profitable? 			   
// Author website:  https://www.thetraderisk.com							   
// Author contact:	contact@thetraderisk.com								   
//==============================================================================

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.BeyondTheCharts
{
	public class Ep001 : Strategy
	{
		private int fast;
		private int slow;
		
		private double startingBalance;
		private double currentBalance;
		private double realizedPnL;
		private double unrealizedPnL;
		
		private SMA smaFast;
		private SMA smaSlow;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Is trading the golden cross 50/200 SMA profitable? | Beyond The Charts Episode 001";
				Name										= "Ep001";
				// Begin initialization of our custom variables 
				fast										= 50;
				slow										= 200;
				startingBalance								= 10000;
				currentBalance								= 10000;
				realizedPnL									= 0.0;
				unrealizedPnL								= 0.0;
				// End initialization of our custom variables
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 2;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 200;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
				// Begin code to plot the 50SMA & 200SMA 
				smaFast = SMA(fast);
				smaSlow = SMA(slow);

				smaFast.Plots[0].Brush = Brushes.Aqua;
				smaSlow.Plots[0].Brush = Brushes.Goldenrod;

				AddChartIndicator(smaFast);
				AddChartIndicator(smaSlow);
				// End code to plot the 50SMA & 200SMA
			}
		}
		// Primary strategy loop called on each update
		protected override void OnBarUpdate()
		{
			// Make sure we have enough data to compute indicators and run strategy logic.
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
			CalculatePerformance();
			BuySellRules();
			OutputPerformance();
		}
		
		// Buy and sell rules
		private void BuySellRules()
		{
			if (CrossAbove(smaFast, smaSlow, 1)) 
				EnterLong(ComputeShareSize(),"Golden Cross Buy");
			else if (CrossBelow(smaFast, smaSlow, 1)) 
				ExitLong("Golden Cross Buy"); 
		}
		
		// Calculate the number of shares to purchase based on running account balance and current closing prices. Assumes 100% invested.
		private int ComputeShareSize()
		{
			return (int)Math.Floor(currentBalance/Close[0]);
		}
		
		// Helper method to track running Pnl and account balance.
		private void CalculatePerformance()
		{
			unrealizedPnL = (Position.GetUnrealizedProfitLoss(PerformanceUnit. Currency, Close[0]));
			realizedPnL = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
			currentBalance = startingBalance + unrealizedPnL + realizedPnL;
		}
		
		// Print strategy PnL to output screen
		private void OutputPerformance()
		{
			PrintTo = PrintTo.OutputTab1;
			Print(Times[0][0].ToString("d"));
			PrintTo = PrintTo.OutputTab2;
			Print(currentBalance.ToString("C0"));
		}
	}
}
