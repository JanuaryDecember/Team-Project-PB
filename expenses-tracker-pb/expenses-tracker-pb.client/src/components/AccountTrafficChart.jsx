import React, { useState, useEffect } from 'react';
import Chart from 'chart.js/auto';
import translation from "../assets/translation.json";

const Chart1 = ({ props }) => {
    const [chart1, setChart1] = useState([]);

    useEffect(() => {
        const fetchChart1 = async () => {
            try {
                const response = await fetch('/api/chart/getChart1', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    credentials: 'include',
                });

                if (response.ok) {
                    const chart1Data = await response.json();
                    
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
            const labels = [translation[props.language].Charts.Incomes, translation[props.language].Charts.Expenditures];
            const data = chart1;


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

        }
    }, [chart1]);

    return (
        <div className="card mx-auto my-5 h-auto background-my w-75">
            <p style={{ fontWeight: 'bold', fontSize: '18px' }}>{translation[props.language].Charts.IncAndExp}</p>
            <canvas id="myChart1" width="520" height="520"></canvas>
        </div>
    );
};

export default Chart1;
