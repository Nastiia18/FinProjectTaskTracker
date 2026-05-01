export const BASE_URL = 'http://localhost:5120'; 

export const headers = {
    'Content-Type': 'application/json',
};

export function getRandomTask() {
    return {
        title: `Task ${Math.floor(Math.random() * 10000)}`,
        description: "Performance test task",
        status: 0,
        priority: 1,
        assigneeId: "02e8234f-3df7-41f3-bf3d-3a98259453c6", // Твій робочий ID
        dueDate: "2026-12-31T23:59:59Z"
    };
}