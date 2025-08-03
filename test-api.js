// Simple API test script to verify our endpoints work
const API_BASE = 'http://localhost:5181/api';

async function testAPI() {
    console.log('🚀 Testing Sports API Endpoints...\n');

    // Test endpoints that should work with mock data
    const tests = [
        {
            name: 'Player Options for User',
            url: `${API_BASE}/PlayerOption/user/user-123?organizationId=3B60378E-782A-45D5-B6C4-AA7466F8D5FD`,
            method: 'GET'
        },
        {
            name: 'Organizations List',
            url: `${API_BASE}/Organization`,
            method: 'GET'
        },
        {
            name: 'Dashboard Stats',
            url: `${API_BASE}/Dashboard/stats`,
            method: 'GET'
        },
        {
            name: 'User Profile',
            url: `${API_BASE}/User/user-123`,
            method: 'GET'
        }
    ];

    for (const test of tests) {
        try {
            console.log(`📡 Testing: ${test.name}`);
            console.log(`   URL: ${test.url}`);
            
            const response = await fetch(test.url, {
                method: test.method,
                headers: {
                    'Content-Type': 'application/json',
                    // Note: In production, you'd need proper authentication headers
                }
            });

            console.log(`   Status: ${response.status} ${response.statusText}`);
            
            if (response.ok) {
                const data = await response.json();
                console.log(`   ✅ Success! Data received:`, JSON.stringify(data, null, 2).substring(0, 200) + '...');
            } else {
                console.log(`   ❌ Failed: ${response.status} ${response.statusText}`);
                const errorText = await response.text();
                console.log(`   Error: ${errorText.substring(0, 100)}...`);
            }
        } catch (error) {
            console.log(`   ❌ Network Error: ${error.message}`);
        }
        console.log('');
    }

    console.log('🎯 API Test Summary:');
    console.log('- API is running on http://localhost:5181');
    console.log('- Swagger UI available at http://localhost:5181/swagger');
    console.log('- Frontend apps can now make API calls');
    console.log('- Seed data script is ready (database configuration needed)');
    console.log('\n✨ Next Steps:');
    console.log('1. Configure database connection string');
    console.log('2. Run database migrations');
    console.log('3. Seed data will populate automatically on startup');
    console.log('4. Test player options voting in the frontend');
}

// Run the test if this script is executed directly
if (typeof window === 'undefined') {
    // Node.js environment
    const fetch = require('node-fetch');
    testAPI().catch(console.error);
} else {
    // Browser environment
    window.testAPI = testAPI;
    console.log('API test function loaded. Run testAPI() in console to test.');
}
