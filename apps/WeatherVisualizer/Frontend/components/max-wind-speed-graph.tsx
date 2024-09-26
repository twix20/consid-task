"use client";

import { useGetWeatherMeasurements } from "@/api/weather-visualizer";
import {
  ResponsiveContainer,
  CartesianGrid,
  XAxis,
  YAxis,
  Tooltip,
  LineChart,
  Line,
  Legend,
} from "recharts";
import { getCityColor, getRandomColor } from "./utils";
import { useMesurmentsToChartData } from "@/hooks/use-mesurments-to-chart-data";

// Mocked data for Max Wind Speed Graph
const maxWindSpeedData = [
  { lastUpdate: "2024-09-26 08:00 AM", Chicago: 18, London: 17, Miami: 15 },
  { lastUpdate: "2024-09-26 09:00 AM", Chicago: 20, London: 18, Miami: 16 },
  { lastUpdate: "2024-09-26 10:00 AM", Chicago: 22, London: 19, Miami: 17 },
  { lastUpdate: "2024-09-26 11:00 AM", Chicago: 25, London: 20, Miami: 18 },
];

export const MaxWindSpeedGraph = () => {
  const { data } = useGetWeatherMeasurements({
    query: {
      refetchInterval: 1000,
    } as never,
  });

  const { chartData, uniqueCityNames } = useMesurmentsToChartData(
    data?.data || []
  );

  return (
    <ResponsiveContainer width="100%" height={400}>
      <LineChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="lastUpdate" />
        <YAxis />
        <Tooltip formatter={(value, name) => [`${value} km/h`, name]} />
        <Legend />

        {uniqueCityNames.map((cityName) => (
          <Line
            key={cityName}
            type="monotone"
            dataKey={`cities.${cityName}.windKph`}
            stroke={getCityColor(cityName)}
            name={cityName}
          />
        ))}
      </LineChart>
    </ResponsiveContainer>
  );
};
