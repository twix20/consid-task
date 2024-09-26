module.exports = {
  petstore: {
    input: {
      target: "http://localhost:57135/swagger/v1/swagger.json",
    },
    output: {
      target: "api/weather-visualizer.ts",
      client: "react-query",
      override: {
        query: {
          useQuery: true,
        },
      },
    },
  },
};
