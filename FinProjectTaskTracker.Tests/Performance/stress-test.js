import http from 'k6/http';
import { check, sleep } from 'k6';
import { BASE_URL, headers, getRandomTask } from './data-helper.js';

export const options = {
    stages: [
        { duration: '2m', target: 50 },
        { duration: '3m', target: 50 },

        { duration: '2m', target: 100 },
        { duration: '3m', target: 100 },

        { duration: '2m', target: 200 },
        { duration: '3m', target: 200 },

        { duration: '2m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<1000'],
        http_req_failed: ['rate<0.05'],
    },
};

export default function () {
    const boardId = '9f08feda-35f4-422b-83e3-109b1cd0da38';
    const payload = JSON.stringify(getRandomTask());

    const res = http.post(`${BASE_URL}/api/boards/${boardId}/tasks`, payload, { headers });

    check(res, {
        'created status 201': (r) => r.status === 201,
    });

    sleep(0.5);
}