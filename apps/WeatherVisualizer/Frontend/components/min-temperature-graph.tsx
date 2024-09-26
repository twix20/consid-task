"use client";

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

// Mocked data for Min Temperature Graph
const minTempData = [
  {
    lastUpdate: "2024-09-26 08:00 AM",
    NewYork: 13,
    Toronto: 11,
    LosAngeles: 18,
  },
  {
    lastUpdate: "2024-09-26 09:00 AM",
    NewYork: 14,
    Toronto: 12,
    LosAngeles: 19,
  },
  {
    lastUpdate: "2024-09-26 10:00 AM",
    NewYork: 15,
    Toronto: 13,
    LosAngeles: 20,
  },
  {
    lastUpdate: "2024-09-26 11:00 AM",
    NewYork: 15.5,
    Toronto: 12.5,
    LosAngeles: 21,
  },
];

export const MinTemperatureGraph = () => {
  return (
    <ResponsiveContainer width="100%" height={400}>
      <LineChart data={minTempData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="lastUpdate" />
        <YAxis />
        <Tooltip formatter={(value, name) => [`${value} °C`, name]} />
        <Legend />
        <Line
          type="monotone"
          dataKey="NewYork"
          stroke="#82ca9d"
          name="New York"
        />
        <Line
          type="monotone"
          dataKey="Toronto"
          stroke="#8884d8"
          name="Toronto"
        />
        <Line
          type="monotone"
          dataKey="LosAngeles"
          stroke="#ff7300"
          name="Los Angeles"
        />
      </LineChart>
    </ResponsiveContainer>
  );
};
