window.algoLabPlotly = {

    // =====================================================
    // 2D COMPLEXITY
    // =====================================================

    drawComplexityChart: function (
        elementId,
        experimental,
        approximation,
        options) {

        const element =
            document.getElementById(elementId);

        if (!element) {
            throw new Error(
                "Не найден элемент графика: " + elementId
            );
        }

        if (typeof Plotly === "undefined") {
            throw new Error("Plotly не загружен.");
        }

        if (!experimental ||
            experimental.length === 0) {
            return;
        }


        const expX =
            experimental.map(p => Number(p.n));

        const expY =
            experimental.map(p => Number(p.time));


        const theoryX =
            approximation.map(p => Number(p.n));

        const theoryY =
            approximation.map(p => Number(p.time));


        const traces = [

            {
                x: expX,
                y: expY,

                type: "scatter",
                mode: "lines+markers",

                name: "Эксперимент",

                line: {
                    color: "#2563eb",
                    width: 3
                },

                marker: {
                    color: "#2563eb",
                    size: 6
                }
            },

            {
                x: theoryX,
                y: theoryY,

                type: "scatter",
                mode: "lines",

                name:
                    options.approximationName
                    || "Аппроксимация",

                line: {
                    color: "#ef4444",
                    width: 4,
                    dash: "dash"
                }
            }
        ];


        Plotly.react(
            element,
            traces,
            {
                title: {
                    text:
                        options.title
                        || "Временная сложность"
                },

                height:
                    options.height
                    || 500,

                xaxis: {
                    title: {
                        text:
                            options.xTitle
                            || "Размер входных данных n"
                    },

                    range: [
                        Math.min(...expX),
                        Math.max(...expX)
                    ],

                    autorange: false
                },

                yaxis: {
                    title: {
                        text:
                            options.yTitle
                            || "Время выполнения, мс"
                    },

                    rangemode: "tozero"
                },

                legend: {
                    orientation: "h",
                    x: 0.5,
                    xanchor: "center",
                    y: -0.2
                },

                margin: {
                    l: 90,
                    r: 40,
                    t: 70,
                    b: 100
                },

                hovermode: "x unified"
            },

            {
                responsive: true,
                displaylogo: false
            }
        );
    },


    // =====================================================
    // MATRIX SURFACE
    // =====================================================

    drawMatrixSurface: function (
        elementId,
        nValues,
        mValues,
        zValues) {

        const element =
            document.getElementById(elementId);

        if (!element) {
            throw new Error(
                "Не найден элемент 3D-графика."
            );
        }

        if (typeof Plotly === "undefined") {
            throw new Error("Plotly не загружен.");
        }


        const trace = {

            x: nValues,
            y: mValues,
            z: zValues,

            type: "surface",

            colorscale: "Viridis",

            colorbar: {
                title: "Время, мс"
            },

            hovertemplate:
                "n = %{x}<br>" +
                "m = %{y}<br>" +
                "Время = %{z:.6f} мс" +
                "<extra></extra>"
        };


        Plotly.react(
            element,
            [trace],

            {
                title: {
                    text:
                        "3D-поверхность времени умножения матриц"
                },

                height: 650,

                scene: {

                    xaxis: {
                        title: {
                            text: "Размер n"
                        }
                    },

                    yaxis: {
                        title: {
                            text: "Размер m"
                        }
                    },

                    zaxis: {
                        title: {
                            text: "Среднее время, мс"
                        }
                    }
                },

                margin: {
                    l: 0,
                    r: 0,
                    b: 0,
                    t: 70
                }
            },

            {
                responsive: true,
                displaylogo: false
            }
        );
    },


    // =====================================================
    // HISTORY COMPARISON
    // =====================================================

    drawHistoryComparison: function (
        elementId,
        series) {

        const element =
            document.getElementById(elementId);

        if (!element) {
            throw new Error(
                "Не найден элемент графика сравнения."
            );
        }

        if (typeof Plotly === "undefined") {
            throw new Error("Plotly не загружен.");
        }

        if (!series ||
            series.length === 0) {
            return;
        }


        const colors = [
            "#2563eb",
            "#ef4444",
            "#22c55e",
            "#f59e0b",
            "#8b5cf6",
            "#06b6d4",
            "#ec4899",
            "#64748b",
            "#84cc16",
            "#f97316"
        ];


        const traces =
            series.map(
                (item, index) => {

                    return {

                        x:
                            item.points.map(
                                p => Number(p.n)),

                        y:
                            item.points.map(
                                p => Number(p.timeMs)),

                        type:
                            "scatter",

                        mode:
                            "lines+markers",

                        name:
                            item.name,

                        line: {
                            width: 3,
                            color:
                                colors[
                                index
                                % colors.length]
                        },

                        marker: {
                            size: 6,
                            color:
                                colors[
                                index
                                % colors.length]
                        },

                        hovertemplate:
                            "n = %{x}<br>" +
                            "Время = %{y:.6f} мс" +
                            "<extra>" +
                            item.name +
                            "</extra>"
                    };
                });


        Plotly.react(
            element,
            traces,

            {
                title: {
                    text:
                        "Сравнение сохранённых экспериментов"
                },

                height: 600,

                xaxis: {
                    title: {
                        text:
                            "Размер входных данных n"
                    }
                },

                yaxis: {
                    title: {
                        text:
                            "Среднее время выполнения, мс"
                    },

                    rangemode:
                        "tozero"
                },

                legend: {
                    orientation:
                        "h",

                    x:
                        0.5,

                    xanchor:
                        "center",

                    y:
                        -0.2
                },

                margin: {
                    l: 90,
                    r: 40,
                    t: 70,
                    b: 120
                },

                hovermode:
                    "closest"
            },

            {
                responsive: true,
                displaylogo: false,
                scrollZoom: true
            }
        );
    },


    purge: function (elementId) {

        const element =
            document.getElementById(elementId);

        if (element &&
            typeof Plotly !== "undefined") {

            Plotly.purge(element);
        }
    }
};