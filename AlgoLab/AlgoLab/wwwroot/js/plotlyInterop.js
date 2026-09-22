window.algoLabPlotly = {

    drawComplexityChart: function (
        elementId,
        experimental,
        approximation,
        options) {

        const element =
            document.getElementById(elementId);


        if (!element) {
            throw new Error(
                "Не найден HTML-элемент графика: "
                + elementId
            );
        }


        if (typeof Plotly === "undefined") {
            throw new Error(
                "Библиотека Plotly не загружена."
            );
        }


        if (!experimental ||
            experimental.length === 0) {

            throw new Error(
                "Нет экспериментальных данных."
            );
        }


        const experimentX =
            experimental.map(
                p => Number(p.n));


        const experimentY =
            experimental.map(
                p => Number(p.time));


        const theoryX =
            approximation.map(
                p => Number(p.n));


        const theoryY =
            approximation.map(
                p => Number(p.time));


        const experimentTrace = {

            x:
                experimentX,

            y:
                experimentY,

            type:
                "scatter",

            mode:
                "lines+markers",

            name:
                "Эксперимент",

            line: {
                color:
                    "#2563eb",

                width:
                    3
            },

            marker: {
                color:
                    "#2563eb",

                size:
                    6
            },

            hovertemplate:
                "n = %{x}<br>" +
                "Время = %{y:.6f} мс" +
                "<extra>Эксперимент</extra>"
        };


        const theoryTrace = {

            x:
                theoryX,

            y:
                theoryY,

            type:
                "scatter",

            mode:
                "lines",

            name:
                options.approximationName
                || "Аппроксимация",

            line: {
                color:
                    "#ef4444",

                width:
                    4,

                dash:
                    "dash"
            },

            hovertemplate:
                "n = %{x}<br>" +
                "Время = %{y:.6f} мс" +
                "<extra>Аппроксимация</extra>"
        };


        const minX =
            Math.min(...experimentX);


        const maxX =
            Math.max(...experimentX);


        const layout = {

            title: {
                text:
                    options.title
                    || "Временная сложность"
            },


            height:
                options.height
                || 500,


            autosize:
                true,


            xaxis: {

                title: {
                    text:
                        options.xTitle
                        || "Размер входных данных n"
                },

                type:
                    "linear",

                range: [
                    minX,
                    maxX
                ],

                autorange:
                    false,

                tickformat:
                    ",d",

                showgrid:
                    true
            },


            yaxis: {

                title: {
                    text:
                        options.yTitle
                        || "Время выполнения, мс"
                },

                rangemode:
                    "tozero",

                showgrid:
                    true
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
                b: 100
            },


            hovermode:
                "x unified"
        };


        const config = {

            responsive:
                true,

            displaylogo:
                false,

            scrollZoom:
                true
        };


        Plotly.react(
            element,
            [
                experimentTrace,
                theoryTrace
            ],
            layout,
            config
        );
    },


    drawMatrix3D: function (
        elementId,
        points) {

        const element =
            document.getElementById(
                elementId);


        if (!element) {
            throw new Error(
                "Не найден элемент 3D-графика."
            );
        }


        if (typeof Plotly === "undefined") {
            throw new Error(
                "Библиотека Plotly не загружена."
            );
        }


        const x =
            points.map(
                p => Number(p.n));


        const y =
            points.map(
                p => Number(p.run));


        const z =
            points.map(
                p => Number(p.timeMs));


        const trace = {

            x: x,

            y: y,

            z: z,

            type:
                "scatter3d",

            mode:
                "markers",

            marker: {

                size:
                    5,

                color:
                    z,

                colorscale:
                    "Viridis",

                opacity:
                    0.9,

                colorbar: {
                    title:
                        "Время, мс"
                }
            }
        };


        const layout = {

            title: {
                text:
                    "3D — умножение матриц"
            },


            height:
                600,


            scene: {

                xaxis: {
                    title: {
                        text:
                            "Размер матрицы n"
                    }
                },

                yaxis: {
                    title: {
                        text:
                            "Номер запуска"
                    }
                },

                zaxis: {
                    title: {
                        text:
                            "Время, мс"
                    }
                }
            }
        };


        Plotly.react(
            element,
            [trace],
            layout,
            {
                responsive:
                    true,

                displaylogo:
                    false
            }
        );
    }
};