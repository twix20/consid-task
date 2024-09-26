import { WeatherMeasurementEntry } from "@/api/weather-visualizer";
import { convertWeatherMeasurementEntriesToGraphData } from "@/components/utils";
import { useMemo } from "react";

export const useMesurmentsToChartData = (
  mesurments: WeatherMeasurementEntry[]
) => {
  return useMemo(() => {
    const chartData = convertWeatherMeasurementEntriesToGraphData(mesurments);

    const uniqueCityNames = Array.from(
      new Set(
        chartData.flatMap((d) =>
          Object.values(d.cities).flatMap((city) => city.name)
        )
      )
    );

    return {
      chartData: chartData,
      uniqueCityNames: uniqueCityNames,
    };
  }, [mesurments]);
};
