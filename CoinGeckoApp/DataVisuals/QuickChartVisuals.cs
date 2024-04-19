using QuickChart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CoinGeckoApp.DataVisuals
{
    public class QuickChartVisuals
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Version { get; set; }
        public string BackgroundColor { get; set; }

        public QuickChartVisuals(int width = 800, int height = 500, string version = "2.9.4", string backgroundColor = "#ffffff")
        {
            Width = width;
            Height = height;
            Version = version;
            BackgroundColor = backgroundColor;
        }

        public string CreateSparkLineURL(double[] data, string borderColor = "black", string? backgroundColor = null)
        {
            string visualType = "sparkline";

            // Create a String Representation for the Array of double
            string dataString = "[" + string.Join(", ", data.Select(d => d.ToString())) + "]";

            string tempBackgroundColor = backgroundColor == null ? HttpUtility.UrlEncode(BackgroundColor) : backgroundColor;

            // Construct the Chart with the Configs
            Chart qc = new Chart();
            qc.Width = Width;
            qc.Height = Height;
            qc.Version = Version;
            qc.Config = 
                $@"{{
				    type: '{visualType}',
				    data: {{
				        datasets: [{{
					        data: {dataString},
                            borderColor: '{borderColor}',
                            backgroundColor: '{tempBackgroundColor}'
				        }}],
				    }}
                }}";

            // Return the "long" url for long-term usage.
            return qc.GetUrl();
        }

        public string CreateLineChartURL(DateTime[] dates, double[] data, string chartLabel, string yAxisLabel)
        {
            if (dates.Length != data.Length) throw new ArgumentException("The lengths of dates and data are not equal!");

            string visualType = "line";

            // Create a String Representation for the Array of double for data
            string dataString = "[" + string.Join(", ", data.Select(d => d.ToString())) + "]";
            
            // Create a String Representation for the Array of Date for data
            string dateString = "[" + 
                string.Join(", ", dates
                .Select(d => d.ToString("MM/dd/yyyy HH:mm"))
                .Select(d => $"new Date('{d}')")) 
                + "]";

            // Construct the Chart with the Configs
            Chart qc = new Chart();
            qc.Width = Width;
            qc.Height = Height;
            qc.Version = Version;
            qc.Config = 
                $@"{{
                    type: '{visualType}',
                    data: {{
                        labels: {dateString},
                        datasets: [{{
                            label: '{chartLabel}',
                            backgroundColor: 'rgba(54, 162, 235, 0.5)',
                            borderColor: 'rgb(54, 162, 235)',
                            fill: false,
                            data: {dataString}
                        }}],
                    }},
                    options: {{
                        scales: {{
                            xAxes: [{{
                                type: 'time',
                                time: {{parser: 'MM/DD/YYYY HH:mm'}},
                                scaleLabel: {{
                                    display: true,
                                    labelString: 'Date'
                                }}
                            }}],
                            yAxes: [{{
                                scaleLabel: {{
                                    display: true,
                                    labelString: '{yAxisLabel}'
                                }}
                            }}],
                        }}
                    }}
                }}";

            return qc.GetUrl();
        }

        public string CreateHorizontalBarChartURL(Dictionary<string, double> labelsAndData, string title, string datasetLabel)
        {
            string visualType = "horizontalBar";

            // Create a String Representation for the Array of string for labels
            string[] labels = labelsAndData.Keys.ToArray();
            string labelsString = "[" + string.Join(", ", labels.Select(d => $"'{d}'")) + "]";

            // Create a String Representation for the Array of double for data
            double[] data = labelsAndData.Values.ToArray();
            string dataString = "[" + string.Join(", ", data.Select(d => d.ToString())) + "]";


            // Construct the Chart with the Configs
            Chart qc = new Chart();
            qc.Width = Width;
            qc.Height = Height;
            qc.Version = Version;
            qc.Config = 
                $@"{{
                    type: '{visualType}',
                    data: {{
                        labels: {labelsString},
                        datasets: [{{
                            label: '{datasetLabel}',
                            backgroundColor: 'rgba(54, 162, 235, 0.5)',
                            borderColor: 'rgb(54, 162, 235)',
                            data: {dataString},
                        }}]
                    }},
                    options: {{
                        elements: {{
                            rectangle: {{borderWidth: 2}}
                        }},
                        responsive: true,
                        legend: {{
                            position: 'right'
                        }},
                        title: {{
                            display: true,
                            text: '{title}'
                        }}
                    }},
                }}";

            return qc.GetUrl();
        }

        public string CreateCandleStickChartURL(
            List<KeyValuePair<DateTime, Tuple<double, double, double, double>>> dataset,
            string tickerSymbol,
            int fontSize = 20,
            string timezone = "UTC-0"
            )
        {

            string visualType = "ohlc";


            // Construct the Chart with the Configs
            Chart qc = new Chart();
            qc.Width = Width;
            qc.Height = Height;
            qc.Version = "3";
            qc.Config = 
                $@"{{
                    type: '{visualType}',
                    data: {{
                        datasets: [{{
                            label: '{tickerSymbol}',
                            yAxisID: 'y',
                            data: {ConvertListKVP2String(dataset)}.map(([d,o,h,l,c]) => ({{x:new Date(d).getTime(),o,h,l,c}})),
                            color: {{
                                up: 'rgba(0, 215, 60, 0.8)',
                                down: 'rgba(255, 0, 0, 0.8)',
                                unchanged: 'rgba(0, 0, 0, 0.8)'
                            }}
                        }}]
                    }},
                    options: {{
                        scales: {{
                            x: {{
                                adapters: {{
                                    date: {{zone: '{timezone}'}}
                                }},
                                time: {{
                                    unit: 'day',
                                    stepSize: 1,
                                    displayFormats: {{
                                        day: 'MMM d',
                                        month: 'MMM d'
                                    }}
                                }}
                            }},
                            y: {{
                                stack: 'stockChart',
                                stackWeight: 10,
                                weight: 2
                            }}
                        }},
                        plugins: {{
                            legend: {{display: false}},
                            title: {{
                                display: true,
                                text: '{tickerSymbol}',
                                font: {{size: {fontSize}}}
                            }}
                        }}
                    }}
                }}";

            return qc.GetUrl();
        }

        private static string ConvertListKVP2String(List<KeyValuePair<DateTime, Tuple<double, double, double, double>>> dataset)
        {
            string outString = "[";

            int i = 0;
            foreach (KeyValuePair<DateTime, Tuple<double, double, double, double>> kvp in dataset)
            {
                DateTime dt = kvp.Key;
                Tuple<double, double, double, double> ohlcvTuple = kvp.Value;

                string tempRecord = "[";
                tempRecord += $"'{dt.ToString("yyyy-MM-dd")}', ";  // Date
                tempRecord += ohlcvTuple.Item1.ToString() + ", ";  // Open
                tempRecord += ohlcvTuple.Item2.ToString() + ", ";  // High
                tempRecord += ohlcvTuple.Item3.ToString() + ", ";  // Low
                tempRecord += ohlcvTuple.Item4.ToString();  // Close
                tempRecord += "]";

                i++;

                if (i >= dataset.Count)  // Last Element
                {
                    outString += tempRecord + "]";
                }
                else  // Not the Last Element
                {
                    outString += tempRecord + ", ";
                }
            }

            return outString;
        }
    }
}
