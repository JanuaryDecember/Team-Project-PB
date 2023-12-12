import React, { useState, useEffect } from 'react';
import Chart from 'chart.js/auto';

const Chart1 = () => {
    const [chart1, setChart1] = useState([]);

    useEffect(() => {
        const fetchChart1 = async () => {
            try {
                const response = await fetch('https://localhost:7088/v1/api/chart/getChart1', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    credentials: 'include',
                });

                if (response.ok) {
                    const chart1Data = await response.json();
                    console.log(chart1Data);
                    setChart1(chart1Data);
                } else {
                    console.error(response);
                    setChart1([]);
                }
            } catch (error) {
                console.error('Error Chart1', error);
            }
        };

        fetchChart1();  
    }, []);

    useEffect(() => {
        if (chart1 && chart1.length > 0) {
            const ctx1 = document.getElementById("myChart1").getContext("2d");
            const labels = ["Incomes", "Expenditures"];
            const data = chart1;

            console.log("Labels:", labels);
            console.log("Data:", data);

            const myChart1 = new Chart(ctx1, {
                type: "pie",
                data: {
                    labels: labels,
                    datasets: [{
                        fill: false,
                        lineTension: 0,
                        backgroundColor: ["pink", "lightblue"],
                        borderColor: "6C698D",
                        data: data,
                    }],
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true,
                        },
                    },
                },
            });

            console.log("myChart1:", myChart1);
        }
    }, [chart1]);

    return (
        <div className="card mx-auto my-5 h-auto background-my w-75">
            <p style={{ fontWeight: 'bold', fontSize: '18px' }}>Chart 1</p>
            <canvas id="myChart1" width="520" height="520"></canvas>
        </div>
    );
};

export default Chart1;
