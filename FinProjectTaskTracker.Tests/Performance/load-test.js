import http from 'k6/http';
import { check, sleep } from 'k6';
import { BASE_URL, headers } from './data-helper.js';

export const options = {
    scenarios: {
        load_test: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '1m', target: 20 },  
                { duration: '3m', target: 20 },  
                { duration: '1m', target: 0 },   
            ],
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'], 
        http_req_failed: ['rate<0.01'],  
    },
};

export default function () {
    const boardId = '9f08feda-35f4-422b-83e3-109b1cd0da38';
    const res = http.get(`${BASE_URL}/api/boards/${boardId}/tasks?status=Todo`, { headers });

    check(res, {
        'status is 200': (r) => r.status === 200,
        'list is returned': (r) => Array.isArray(r.json()),
    });

    sleep(1); 
}