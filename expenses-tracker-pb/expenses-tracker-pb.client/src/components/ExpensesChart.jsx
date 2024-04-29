import React, { useEffect, useState } from 'react';
import Chart from 'chart.js/auto';

const ExpensesChart = ({ walletId, transactions }) => {
    const [chartData, setChartData] = useState(null);
    const [chartInstance, setChartInstance] = useState(null);

    useEffect(() => {
        fetchData();
    }, [walletId, transactions]);

    useEffect(() => {
        if (chartData) {
            if (chartInstance) {
                chartInstance.destroy();
            }
            renderChart();
        }
    }, [chartData]);

    const fetchData = async () => {
        try {
            const response = await fetch(`/api/chart/GetExpensesChart?&walletId=${walletId}`);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const data = await response.json();
            setChartData(data);
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    const renderChart = () => {
        const ctx = document.getElementById('expensesChart').getContext('2d');

        const chartConfig = {
            type: 'doughnut',
            data: {
                labels: Object.keys(chartData),
                datasets: [{
                    label: 'Expenses by Category',
                    data: Object.values(chartData),
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.2)',
                        'rgba(54, 162, 235, 0.2)',
                        'rgba(255, 206, 86, 0.2)',
                        'rgba(75, 192, 192, 0.2)',
                        'rgba(153, 102, 255, 0.2)',
                        'rgba(255, 159, 64, 0.2)',
                        'rgba(255, 0, 0, 0.2)',   
                        'rgba(0, 255, 0, 0.2)',   
                        'rgba(0, 0, 255, 0.2)',   
                        'rgba(255, 255, 0, 0.2)',
                    ],
                    borderColor: [
                        'rgba(255, 99, 132, 1)',
                        'rgba(54, 162, 235, 1)',
                        'rgba(255, 206, 86, 1)',
                        'rgba(75, 192, 192, 1)',
                        'rgba(153, 102, 255, 1)',
                        'rgba(255, 159, 64, 1)',
                        'rgba(255, 0, 0, 1)',     
                        'rgba(0, 255, 0, 1)',     
                        'rgba(0, 0, 255, 1)',     
                        'rgba(255, 255, 0, 1)',   
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom',
                    },
                }
            }
        };

        const newChartInstance = new Chart(ctx, chartConfig);
        setChartInstance(newChartInstance);
    };

    return (
        <div className="card mx-auto my-5 h-auto background-chart w-75" style={{ border: 'none' }}>
            <p style={{ fontWeight: 'bold', fontSize: '22px', textAlign: 'center', marginBottom: '20px' }}>Expenses by Category</p>
            <div style={{ height: '330px' }}>
                <canvas id="expensesChart" width="400" height="400"></canvas>
            </div>
        </div>
    );
};

export default ExpensesChart;
