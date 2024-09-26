"use client";

import { useGetWeatherMeasurements } from "@/api/weather-visualizer";
import { useMesurmentsToChartData } from "@/hooks/use-mesurments-to-chart-data";
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
import { getCityColor } from "./utils";

export const MinTemperatureGraph = () => {
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
        <Tooltip formatter={(value, name) => [`${value} °C`, name]} />
        <Legend />

        {uniqueCityNames.map((cityName) => (
          <Line
            key={cityName}
            type="monotone"
            dataKey={`cities.${cityName}.temperatureC`}
            stroke={getCityColor(cityName)}
            name={cityName}
          />
        ))}
      </LineChart>
    </ResponsiveContainer>
  );
};
