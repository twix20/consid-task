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

// Mocked data for Max Wind Speed Graph
const maxWindSpeedData = [
  { lastUpdate: "2024-09-26 08:00 AM", Chicago: 18, London: 17, Miami: 15 },
  { lastUpdate: "2024-09-26 09:00 AM", Chicago: 20, London: 18, Miami: 16 },
  { lastUpdate: "2024-09-26 10:00 AM", Chicago: 22, London: 19, Miami: 17 },
  { lastUpdate: "2024-09-26 11:00 AM", Chicago: 25, London: 20, Miami: 18 },
];

export const MaxWindSpeedGraph = () => {
  return (
    <ResponsiveContainer width="100%" height={400}>
      <LineChart data={maxWindSpeedData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="lastUpdate" />
        <YAxis />
        <Tooltip formatter={(value, name) => [`${value} km/h`, name]} />
        <Legend />
        <Line
          type="monotone"
          dataKey="Chicago"
          stroke="#82ca9d"
          name="Chicago"
        />
        <Line type="monotone" dataKey="London" stroke="#8884d8" name="London" />
        <Line type="monotone" dataKey="Miami" stroke="#ff7300" name="Miami" />
      </LineChart>
    </ResponsiveContainer>
  );
};
