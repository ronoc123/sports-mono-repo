// Test CORS configuration
const API_BASE = 'http://localhost:5181';

async function testCORS() {
    console.log('🌐 Testing CORS Configuration...\n');

    try {
        // Test preflight request (OPTIONS)
        console.log('1. Testing preflight request (OPTIONS)...');
        const preflightResponse = await fetch(`${API_BASE}/Org/GetAllOrganization`, {
            method: 'OPTIONS',
            headers: {
                'Origin': 'http://localhost:4200',
                'Access-Control-Request-Method': 'GET',
                'Access-Control-Request-Headers': 'Content-Type'
            }
        });

        console.log(`   Preflight Status: ${preflightResponse.status}`);
        console.log('   Preflight Headers:');
        preflightResponse.headers.forEach((value, key) => {
            if (key.toLowerCase().includes('access-control')) {
                console.log(`     ${key}: ${value}`);
            }
        });

        // Test actual request
        console.log('\n2. Testing actual GET request...');
        const response = await fetch(`${API_BASE}/Org/GetAllOrganization`, {
            method: 'GET',
            headers: {
                'Origin': 'http://localhost:4200',
                'Content-Type': 'application/json'
            }
        });

        console.log(`   Request Status: ${response.status}`);
        console.log('   Response Headers:');
        response.headers.forEach((value, key) => {
            if (key.toLowerCase().includes('access-control')) {
                console.log(`     ${key}: ${value}`);
            }
        });

        if (response.ok) {
            const data = await response.json();
            console.log(`\n✅ CORS is working! Found ${data.data?.items?.length || 0} organizations`);
        } else {
            console.log(`\n❌ Request failed with status: ${response.status}`);
        }

    } catch (error) {
        console.log('❌ CORS Test Error:', error.message);
    }
}

// Test from different origins
async function testMultipleOrigins() {
    console.log('\n🔄 Testing multiple origins...\n');
    
    const origins = [
        'http://localhost:4200',
        'http://localhost:3000',
        'http://localhost:5173',
        'https://localhost:4200'
    ];

    for (const origin of origins) {
        console.log(`Testing origin: ${origin}`);
        try {
            const response = await fetch(`${API_BASE}/Org/GetAllOrganization`, {
                method: 'GET',
                headers: {
                    'Origin': origin,
                    'Content-Type': 'application/json'
                }
            });
            
            const corsHeader = response.headers.get('Access-Control-Allow-Origin');
            console.log(`  Status: ${response.status}, CORS Header: ${corsHeader || 'Not set'}`);
        } catch (error) {
            console.log(`  Error: ${error.message}`);
        }
    }
}

// Run tests
console.log('🚀 Starting CORS Tests...\n');
testCORS()
    .then(() => testMultipleOrigins())
    .catch(console.error);
