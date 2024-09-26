import { WeatherMeasurementEntry } from "@/api/weather-visualizer";

export const getRandomColor = () => {
  let randomColor = "#" + Math.floor(Math.random() * 16777215).toString(16);
  return randomColor;
};

const cityColorsCache: Record<string, string> = {};
export const getCityColor = (cityName: string) => {
  if (!(cityName in cityColorsCache)) {
    cityColorsCache[cityName] = getRandomColor();
  }

  return cityColorsCache[cityName];
};

export const convertWeatherMeasurementEntriesToGraphData = (
  response: WeatherMeasurementEntry[]
) => {
  const groupedData = {} as Record<
    string,
    {
      lastUpdate: string;
      cities: Record<
        string,
        {
          name: string;
          temperatureC: number;
          windKph: number;
        }
      >;
    }
  >;

  response.forEach((item) => {
    // Parse and format the date to "YYYY-MM-DD hh:mm AM/PM" format
    const dateObj = new Date(item.lastUpdated!);
    const formattedDate = dateObj
      .toLocaleString("en-US", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        hour12: true,
      })
      .replace(",", ""); // Removing extra comma for cleaner format

    const cityInfo = {
      name: item.city!,
      temperatureC: item.temperatureC!,
      windKph: item.windKph!,
    };

    if (!groupedData[formattedDate]) {
      groupedData[formattedDate] = {
        lastUpdate: formattedDate,
        cities: {},
      };
    }

    groupedData[formattedDate].cities[cityInfo.name] = cityInfo;
  });

  return Object.values(groupedData).sort(
    (a, b) =>
      new Date(a.lastUpdate).valueOf() - new Date(b.lastUpdate).valueOf()
  );
};
